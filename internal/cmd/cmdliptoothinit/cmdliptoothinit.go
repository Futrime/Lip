package cmdliptoothinit

import (
	"bufio"
	"flag"
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/tooth"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth init [options]

Description:
  Initialize and writes a new tooth.json file in the current directory, in effect creating a new tooth rooted at the current directory.

Options:
  -h, --help                  Show help.
`

var metadataTemplate = tooth.RawMetadata{
	FormatVersion: 2,
	Tooth:         "",
	Version:       "0.0.0",
	Info: tooth.RawMetadataInfo{
		Name:        "",
		Description: "",
		Author:      "",
		Tags:        []string{},
	},
}

func Run(ctx *context.Context, args []string) error {

	flagSet := flag.NewFlagSet("init", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err := flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags\n\t%w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	if err := initTooth(ctx); err != nil {
		return fmt.Errorf("failed to initialize the tooth\n\t%w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// initTooth initializes a new tooth in the current directory.
func initTooth(ctx *context.Context) error {

	// Check if tooth.json already exists.
	_, err := os.Stat("tooth.json")
	if err == nil {
		return fmt.Errorf("tooth.json already exists")
	}

	rawMetadata := metadataTemplate

	// Ask for information.
	var ans string
	scanner := bufio.NewScanner(os.Stdin)

	log.Info("What is the tooth repo path? (e.g. github.com/tooth-hub/llbds3)")
	scanner.Scan()
	ans = scanner.Text()

	if !tooth.IsValidToothRepoPath(ans) {
		return fmt.Errorf("invalid tooth repo path %v\n\t%w", ans, err)
	}

	rawMetadata.Tooth = ans

	log.Info("What is the name?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Name = ans

	log.Info("What is the description?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Description = ans

	log.Info("What is the author? Please input your GitHub username.")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Author = ans

	metadata, err := tooth.MakeMetadataFromRaw(rawMetadata)
	if err != nil {
		return fmt.Errorf("failed to make metadata\n\t%w", err)
	}

	jsonBytes, err := metadata.MarshalJSON()
	if err != nil {
		return fmt.Errorf("failed to marshal metadata\n\t%w", err)
	}

	// Create tooth.json.
	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return fmt.Errorf("failed to get workspace directory\n\t%w", err)
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return fmt.Errorf("failed to parse workspace directory\n\t%w", err)
	}

	file, err := os.Create(workspaceDir.Join(path.MustParse("tooth.json")).LocalString())
	if err != nil {
		return fmt.Errorf("failed to create tooth.json\n\t%w", err)
	}
	defer file.Close()

	// Write default tooth.json content.
	if _, err := file.Write(jsonBytes); err != nil {
		return fmt.Errorf("failed to write tooth.json\n\t%w", err)
	}

	log.Info("Successfully initialized a new tooth.")

	return nil
}
