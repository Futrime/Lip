// Package metadata contains the metadata of a tooth.
package toothmetadata

import (
	"bytes"
	"encoding/json"
	"errors"
	"regexp"
	"strings"

	"github.com/liteldev/lip/tooth"
	versionutils "github.com/liteldev/lip/utils/version"
	"github.com/liteldev/lip/utils/version/versionmatch"
	"github.com/xeipuuv/gojsonschema"
)

// InfoStruct is the struct that contains the information of a tooth.
type InfoStruct struct {
	Name        string
	Description string
	Author      string
	License     string
	Homepage    string
}

// PlacementStruct is the struct that contains the source and destination of a placement.
type PlacementStruct struct {
	Source      string
	Destination string
}

// CommandStruct is the struct that contains the type, commands, GOOS and GOARCH of a command.
type CommandStruct struct {
	Type     string
	Commands []string
	GOOS     string
	GOARCH   string
}

// Metadata is the struct that contains all the metadata of a tooth.
type Metadata struct {
	ToothPath    string
	Version      versionutils.Version
	Dependencies map[string]([][]versionmatch.VersionMatch)
	Information  InfoStruct
	Placement    []PlacementStruct
	Possession   []string
	Commands     []CommandStruct
}

const jsonSchema string = `
{
  "$schema": "https://json-schema.org/draft-07/schema",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "format_version",
    "tooth",
    "version"
  ],
  "properties": {
    "format_version": {
      "enum": [1]
    },
    "tooth": {
      "type": "string",
      "pattern": "^[a-zA-Z\\d-_\\.\\/]*$"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.(\\d+|0-[a-z]+(\\.[0-9]+)?)$"
    },
    "dependencies": {
      "type": "object",
      "additionalProperties": false,
      "patternProperties": {
        "^[a-zA-Z\\d-_\\.\\/]*$": {
          "type": "array",
          "uniqueItems": true,
          "minItems": 1,
          "additionalItems": false,
          "items": {
            "type": "array",
            "uniqueItems": true,
            "minItems": 1,
            "additionalItems": false,
            "items": {
              "type": "string",
              "pattern": "^((>|>=|<|<=|!)?\\d+\\.\\d+\\.\\d+|\\d+\\.\\d+\\.x)$"
            }
          }
        }
      }
    },
    "information": {
      "type": "object"
    },
    "placement": {
      "type": "array",
      "additionalItems": false,
      "items": {
        "type": "object",
        "additionalProperties": false,
        "properties": {
          "source": {
            "type": "string",
            "pattern": "^[a-zA-Z0-9-_]([a-zA-Z0-9-_\\.\/]*([a-zA-Z0-9-_]|\\/\\*))?$"
          },
          "destination": {
            "type": "string",
            "pattern": "^[a-zA-Z0-9-_]([a-zA-Z0-9-_\\.\/]*([a-zA-Z0-9-_]|\\/\\*))?$"
          }
        }
      }
    },
    "possession": {
      "type": "array",
      "additionalItems": false,
      "items": {
        "type": "string",
        "pattern": "^[a-zA-Z0-9-_][a-zA-Z0-9-_\\.\/]*\\/$"
      }
    },
    "commands": {
      "type": "array",
      "items": {
        "type": "object",
        "additionalProperties": false,
        "required": [
          "type",
          "commands",
          "GOOS"
        ],
        "properties": {
          "type": {
            "enum": ["install", "uninstall"]
          },
          "commands": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "GOOS": {
            "type": "string"
          },
          "GOARCH": {
            "type": "string"
          }
        }
      }
    }
  }
}
`

// NewFromJSON decodes a JSON byte array into a Metadata struct.
func NewFromJSON(jsonData []byte) (Metadata, error) {
	// Validate JSON schema.
	schemaLoader := gojsonschema.NewStringLoader(jsonSchema)
	documentLoader := gojsonschema.NewBytesLoader(jsonData)

	result, err := gojsonschema.Validate(schemaLoader, documentLoader)
	if err != nil {
		return Metadata{}, errors.New("JSON schema validation failed: " + err.Error())
	}

	if !result.Valid() {
		var errorString string
		for _, desc := range result.Errors() {
			errorString += desc.String() + " "
		}
		return Metadata{}, errors.New("JSON schema validation failed: " + errorString)
	}

	// Read to a map.
	var metadataMap map[string]interface{}
	err = json.Unmarshal(jsonData, &metadataMap)
	if err != nil {
		return Metadata{}, errors.New("failed to decode JSON into metadata: " + err.Error())
	}

	// Parse to metadata.
	var metadata Metadata

	// Tooth path should be lower case.
	metadata.ToothPath = strings.ToLower(metadataMap["tooth"].(string))
	if !tooth.IsValidToothPath(metadata.ToothPath) {
		return Metadata{}, errors.New("failed to decode JSON into metadata: invalid tooth path: " + metadata.ToothPath)
	}

	version, err := versionutils.NewFromString(metadataMap["version"].(string))

	if err != nil {
		return Metadata{}, errors.New("failed to decode JSON into metadata: " + err.Error())
	}
	metadata.Version = version

	metadata.Dependencies = make(map[string]([][]versionmatch.VersionMatch))
	if _, ok := metadataMap["dependencies"]; ok {
		for toothPath, versionMatchOuterList := range metadataMap["dependencies"].(map[string]interface{}) {
			metadata.Dependencies[toothPath] = make([][]versionmatch.VersionMatch, len(versionMatchOuterList.([]interface{})))
			for i, versionMatchInnerList := range versionMatchOuterList.([]interface{}) {
				metadata.Dependencies[toothPath][i] = make([]versionmatch.VersionMatch, len(versionMatchInnerList.([]interface{})))
				for j, versionMatch := range versionMatchInnerList.([]interface{}) {
					versionMatch, err := versionmatch.NewFromString(versionMatch.(string))
					if err != nil {
						return Metadata{}, errors.New("failed to decode JSON into metadata: " + err.Error())
					}

					metadata.Dependencies[toothPath][i][j] = versionMatch
				}
			}
		}
	}

	if _, ok := metadataMap["information"]; ok {
		if _, ok := metadataMap["information"].(map[string]interface{})["name"]; ok {
			metadata.Information.Name = metadataMap["information"].(map[string]interface{})["name"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["description"]; ok {
			metadata.Information.Description = metadataMap["information"].(map[string]interface{})["description"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["author"]; ok {
			metadata.Information.Author = metadataMap["information"].(map[string]interface{})["author"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["license"]; ok {
			metadata.Information.License = metadataMap["information"].(map[string]interface{})["license"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["homepage"]; ok {
			metadata.Information.Homepage = metadataMap["information"].(map[string]interface{})["homepage"].(string)
		}
	}

	if _, ok := metadataMap["placement"]; ok {
		metadata.Placement = make([]PlacementStruct, len(metadataMap["placement"].([]interface{})))
		for i, placement := range metadataMap["placement"].([]interface{}) {
			source := placement.(map[string]interface{})["source"].(string)
			destination := placement.(map[string]interface{})["destination"].(string)

			// Source and destination should starts with a letter or a digit and should only contains
			reg := regexp.MustCompile(`^[a-zA-Z0-9]\S*$`)
			// The matched string should be the same as the original string.
			if reg.FindString(source) != source {
				return Metadata{}, errors.New("failed to decode JSON into metadata: invalid source: " + source)
			}
			if reg.FindString(destination) != destination {
				return Metadata{}, errors.New("failed to decode JSON into metadata: invalid destination: " + destination)
			}

			metadata.Placement[i].Source = source
			metadata.Placement[i].Destination = destination
		}
	} else {
		metadata.Placement = make([]PlacementStruct, 0)
	}

	if _, ok := metadataMap["possession"]; ok {
		metadata.Possession = make([]string, len(metadataMap["possession"].([]interface{})))
		for i, possession := range metadataMap["possession"].([]interface{}) {
			metadata.Possession[i] = possession.(string)
		}
	} else {
		metadata.Possession = make([]string, 0)
	}

	if _, ok := metadataMap["commands"]; ok {
		metadata.Commands = make([]CommandStruct, len(metadataMap["commands"].([]interface{})))
		for i, command := range metadataMap["commands"].([]interface{}) {
			commandType := command.(map[string]interface{})["type"].(string)
			commandContent := make([]string, len(command.(map[string]interface{})["commands"].([]interface{})))
			for j, command := range command.(map[string]interface{})["commands"].([]interface{}) {
				commandContent[j] = command.(string)
			}
			commandGOOS := command.(map[string]interface{})["GOOS"].(string)
			commandGOARCH := ""
			if _, ok := command.(map[string]interface{})["GOARCH"]; ok {
				commandGOARCH = command.(map[string]interface{})["GOARCH"].(string)
			}

			metadata.Commands[i].Type = commandType
			metadata.Commands[i].Commands = commandContent
			metadata.Commands[i].GOOS = commandGOOS
			metadata.Commands[i].GOARCH = commandGOARCH
		}
	} else {
		metadata.Commands = make([]CommandStruct, 0)
	}

	return metadata, nil
}

// JSON encodes a Metadata struct into a JSON byte array.
func (metadata Metadata) JSON() ([]byte, error) {
	metadataMap := make(map[string]interface{})

	metadataMap["tooth"] = metadata.ToothPath

	metadataMap["version"] = metadata.Version.String()

	metadataMap["dependencies"] = make(map[string]interface{})
	for toothPath, versionMatchOuterList := range metadata.Dependencies {
		metadataMap["dependencies"].(map[string]interface{})[toothPath] =
			make([]interface{}, len(versionMatchOuterList))
		for i, versionMatchInnerList := range versionMatchOuterList {
			metadataMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i] =
				make([]interface{}, len(versionMatchInnerList))
			for j, versionMatch := range versionMatchInnerList {
				metadataMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i].([]interface{})[j] = versionMatch.String()
			}
		}
	}

	metadataMap["information"] = make(map[string]interface{})
	metadataMap["information"].(map[string]interface{})["name"] = metadata.Information.Name
	metadataMap["information"].(map[string]interface{})["description"] = metadata.Information.Description
	metadataMap["information"].(map[string]interface{})["author"] = metadata.Information.Author
	metadataMap["information"].(map[string]interface{})["license"] = metadata.Information.License
	metadataMap["information"].(map[string]interface{})["homepage"] = metadata.Information.Homepage

	metadataMap["placement"] = make([]interface{}, len(metadata.Placement))
	for i, placement := range metadata.Placement {
		metadataMap["placement"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["placement"].([]interface{})[i].(map[string]interface{})["source"] = placement.Source
		metadataMap["placement"].([]interface{})[i].(map[string]interface{})["destination"] = placement.Destination
	}

	metadataMap["possession"] = make([]interface{}, len(metadata.Possession))
	for i, possession := range metadata.Possession {
		metadataMap["possession"].([]interface{})[i] = possession
	}

	metadataMap["commands"] = make([]interface{}, len(metadata.Commands))
	for i, command := range metadata.Commands {
		metadataMap["commands"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["type"] = command.Type
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["commands"] = command.Commands
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["GOOS"] = command.GOOS
		if command.GOARCH != "" {
			metadataMap["commands"].([]interface{})[i].(map[string]interface{})["GOARCH"] = command.GOARCH
		}
	}

	// Encode metadataMap into JSON
	buf := bytes.NewBuffer([]byte{})
	encoder := json.NewEncoder(buf)

	encoder.SetIndent("", "  ")

	// Prevent HTML escaping. Otherwise, "<", ">", "&", U+2028, and U+2029
	// characters are escaped to "\u003c", "\u003e", "\u0026", "\u2028", and "\u2029".
	encoder.SetEscapeHTML(false)

	err := encoder.Encode(metadataMap)
	if err != nil {
		return nil, errors.New("failed to encode metadata into JSON: " + err.Error())
	}

	return buf.Bytes(), nil
}
