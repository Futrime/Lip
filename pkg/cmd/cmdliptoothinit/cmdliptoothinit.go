package cmdliptoothinit

import (
	"bufio"
	"errors"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/teeth"
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

const toothJsonTemplate = `{
	"format_version": 2,
	"tooth": "",
	"version": "0.0.0",
	"info": {
		"name": "",
		"description": "",
		"author": ""
	}
}
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("init", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logging.Info(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	err = initTooth(ctx)
	if err != nil {
		return fmt.Errorf("failed to initialize the tooth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// initTooth initializes a new tooth in the current directory.
func initTooth(ctx contexts.Context) error {
	var err error

	// Check if tooth.json already exists.
	_, err = os.Stat("tooth.json")
	if err == nil {
		return fmt.Errorf("tooth.json already exists")
	}

	rawMetadata, err := teeth.NewRawMetadata([]byte(toothJsonTemplate))
	if err != nil {
		return errors.New("failed to create a new tooth rawMetadata")
	}

	// Ask for information.
	var ans string
	scanner := bufio.NewScanner(os.Stdin)

	logging.Info("What is the tooth path? (e.g. github.com/tooth-hub/llbds3)")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Tooth = ans

	// To lower case.
	rawMetadata.Tooth = strings.ToLower(rawMetadata.Tooth)

	logging.Info("What is the name?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Name = ans

	logging.Info("What is the description?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Description = ans

	logging.Info("What is the author? Please input your GitHub username.")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Author = ans

	toothJsonBytes, err := rawMetadata.JSON()
	if err != nil {
		return errors.New("failed to convert tooth rawMetadata to JSON")
	}

	_, err = teeth.NewMetadata(toothJsonBytes)
	if err != nil {
		return errors.New("some information is invalid: " + err.Error())
	}

	// Create tooth.json.
	workspaceDir, err := ctx.WorkspaceDir()
	if err != nil {
		return errors.New("failed to get workspace directory")
	}

	file, err := os.Create(filepath.Join(workspaceDir, "tooth.json"))
	if err != nil {
		return errors.New("failed to create tooth.json")
	}
	defer file.Close()

	// Write default tooth.json content.
	_, err = file.WriteString(string(toothJsonBytes))
	if err != nil {
		return errors.New("failed to write tooth.json")
	}

	logging.Info("Successfully initialized a new tooth.")

	return nil
}
