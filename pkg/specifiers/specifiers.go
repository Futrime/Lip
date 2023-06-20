package specifiers

import (
	"fmt"
	"regexp"
	"strings"

	"github.com/lippkg/lip/pkg/versions"
)

// SpecifierKind is an enum that represents the type of a specifier.
type SpecifierKind int

const (
	ToothArchiveKind SpecifierKind = iota
	ToothRepoKind
)

// Specifier is a type that can be used to specify a tooth url/file or a requirement.
type Specifier struct {
	specifierKind           SpecifierKind
	toothArchivePath        string
	toothRepo               string
	toothVersion            versions.Version
	isToothVersionSpecified bool
}

// New creates a new specifier.
func New(specifierString string) (Specifier, error) {
	var err error

	specifierType := getSpecifierType(specifierString)

	switch specifierType {
	case ToothArchiveKind:
		return Specifier{
			specifierKind:    specifierType,
			toothArchivePath: specifierString,
		}, nil

	case ToothRepoKind:
		// tooth repo strings should be lower case.
		specifierString = strings.ToLower(specifierString)

		reg := regexp.MustCompile(
			`^[a-z0-9][a-z0-9-_\.\/]*(@\d+\.\d+\.\d+(-[a-z]+(\.\d+)?)?)?$`)

		// If not matched or the matched string is not the same as the specifier, it is an
		// invalid requirement specifier.
		if reg.FindString(specifierString) != specifierString {
			return Specifier{}, fmt.Errorf("invalid requirement specifier: %v",
				specifierString)
		}

		// Parse the tooth repo and version.
		splittedSpecifier := strings.Split(specifierString, "@")

		toothRepo := splittedSpecifier[0]

		var toothVersion versions.Version

		if len(splittedSpecifier) == 2 {
			toothVersion, err = versions.NewFromString(splittedSpecifier[1])
			if err != nil {
				return Specifier{}, fmt.Errorf("invalid requirement specifier: %v",
					specifierString)
			}

			return Specifier{
				specifierKind:           specifierType,
				toothRepo:               toothRepo,
				toothVersion:            toothVersion,
				isToothVersionSpecified: true,
			}, nil

		} else {
			return Specifier{
				specifierKind:           specifierType,
				toothRepo:               toothRepo,
				isToothVersionSpecified: false,
			}, nil
		}
	}

	// Never reached.
	panic("unreachable")
}

// IsToothVersionSpecified returns whether the specifier has a tooth version.
func (s Specifier) IsToothVersionSpecified() bool {
	return s.specifierKind == ToothRepoKind && s.isToothVersionSpecified
}

// String returns the string representation of the specifier.
func (s Specifier) String() string {
	switch s.specifierKind {
	case ToothArchiveKind:
		return s.toothArchivePath

	case ToothRepoKind:
		return s.toothRepo + "@" + s.toothVersion.String()
	}

	// Never reached.
	panic("unreachable")
}

// ToothArchivePath returns the path of the tooth archive.
func (s Specifier) ToothArchivePath() (string, error) {
	if s.Type() != ToothArchiveKind {
		return "", fmt.Errorf("specifier is not a tooth archive")
	}

	return s.toothArchivePath, nil
}

// ToothRepo returns the tooth repo of the specifier.
func (s Specifier) ToothRepo() (string, error) {
	if s.Type() != ToothRepoKind {
		return "", fmt.Errorf("specifier is not a tooth repo")
	}

	return s.toothRepo, nil
}

// ToothVersion returns the version of the tooth.
func (s Specifier) ToothVersion() (versions.Version, error) {
	if s.Type() != ToothRepoKind {
		return versions.Version{}, fmt.Errorf("specifier is not a tooth repo")
	}

	if !s.isToothVersionSpecified {
		return versions.Version{}, fmt.Errorf("tooth version is not specified")
	}

	return s.toothVersion, nil
}

// Type returns the type of the specifier.
func (s Specifier) Type() SpecifierKind {
	return s.specifierKind
}

// ---------------------------------------------------------------------

// getSpecifierType gets the type of the requirement specifier.
func getSpecifierType(specifier string) SpecifierKind {
	if strings.HasSuffix(specifier, ".tth") {
		return ToothArchiveKind
	} else {
		return ToothRepoKind
	}
}
