# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.14.2] - 2023-04-24

### Fixed

- Remove misleading message when dependencies are satisfied.
- Packed tooth file not able to be installed.

## [0.14.1] - 2023-04-20

### Fixed

- Broken GOOS and GOARCH specifiers of placement.

## [0.14.0] - 2023-03-24

### Added

- `lip tooth pack` command to pack a tooth into a tooth file.
- Questions when initializing a tooth.

### Fixed

- Not properly parsing placement sometimes.
- Not aborting when a tooth fails to install.

## [0.13.0] - 2023-03-05

### Added

- Aliases for subcommands.

### Changed

- Tool should be registered in `tooth.json` manually now.

## [0.12.0] - 2023-02-27

### Added

- Dependency version validation when installing.
- `lip autoremove` command to remove tooths not required by other tooths.

### Fixed

- Failing to run tools with arguments.
- Dependencies still being installed when the dependent is not going to be installed.

## [0.11.45141] - 2023-02-19

### Fixed

- Wrongly showing debug information when redirecting to local Lip.

## [0.11.4514] - 2023-02-17

### Added

- Showing information about installed tooths in `lip list` command.

## [0.11.0] - 2023-02-17

### Added

- `--available` flag for `lip show` command.
- `--numeric-progress` flag for `lip install` command.
- `--no-dependencies` flag for `lip install` command.
- `confirmation` field in `tooth.json` to show messages and ask for confirmation before installing.
- Check for invalid additional arguments.
- Structured information output.
- Support for multiple GOPROXYs.
- `--keep-possession` flag for `lip uninstall` command.
- Automatic deletion of empty directories when uninstalling a tooth.
- Support for file possession.

### Fixed

- Remove wrongly displayed debug information.
- Failing to re-download when a broken tooth file exists in cache.

## [0.10.0] - 2023-02-12

### Added

- Verbose and quiet mode.
- JSON output support for `lip list` and `lip show`.

## [0.9.0] - 2023-02-11

### Added

- HTTP Code reporting when failing to make a request.
- `lip list --upgradable` command.
- Topological sorting for dependencies.
- Progress bar for downloading tooth files.

### Fixed

- No notice when a tooth file is cached.
- Tooth paths in `dependencies` field of `tooth.json` not converting to lowercase.
- Mistakes in help message of `lip cache purge`.

## [0.8.3] - 2023-02-09

### Fixed

- Mistakes in path prefix extraction when there is only one file in the tooth.

## [0.8.2] - 2023-02-09

### Fixed

- Failing to input anything to post-install and pre-uninstall commands.
- Wrong installation order of dependencies.
- Registry not working in `lip show`.
- Unstable versions can be wrongly installed when no specific version is specified.

## [0.8.1] - 2023-02-07

### Fixed

- Failing to get information from registry with other index than index.json.

## [0.8.0] - 2023-02-06

### Added

- Registry support. The default registry is <https://registry.litebds.com>.

### Fixed

- Failing to uninstall tooths with uppercase letters in provided tooth path.

## [0.7.1] - 2023-02-05

### Fixed

- Failing to hot update or remove Lip in local directory.

## [0.7.0] - 2023-02-01

### Added

- Support for installing anything to any path.
- Prompt for confirmation when installing to a path that is not in working directory.

## [0.6.0] - 2023-01-31

### Added

- Support for on-demand installation depending on OS and platform.
- Removal for downloaded tooth files that do not pass the validation.

## [0.5.1] - 2023-01-30

### Fixed

- Failing to install any tool.

## [0.5.0] - 2023-01-30

### Added

- Available version list in `lip show` command.
- Redirection to local lip executable when running `lip`.
- Support for pre-uninstall scripts.
- Support for hot update of lip.
- Support for executing tools in `.lip/tools` directory.

## [0.4.0] - 2023-01-26

### Added

- Post-install script support.
- Tooth path validation.
- Flexible tooth.json parsing.

## [0.3.4] - 2023-01-25

### Changed

- Bumped github.com/fatih/color from 1.14.0 to 1.14.1.

### Fixed

- Misleading error hints.
- Failing to fetch tooth with major version v0 or v1.
- Failing to match dependencies.
- Failing to fetch tooth when uppercase letters exist in tooth path.

## [0.3.3] - 2023-01-24

### Fixed

- Default to earliest version when no version is specified in tooth.json.
- Panic when tooth.json is invalid.

## [0.3.2] - 2023-01-23

### Added

- "Add to PATH" option in setup utility.
- Mac OS, Linux and OpenBSD support.
- Arm64 support.

## [0.3.1] - 2023-01-21

### Added

- Setup utility to install Lip.

## [0.3.0] - 2023-01-20

### Added

- Possession keeping support when force-reinstalling or upgrading.
- `--force-reinstall` flag and `--upgrade` flag support.

## [0.2.1] - 2023-01-18

### Fixed

- Failing to fetch tooth whose version has suffix `+incompatible`.
- Failing to parse wildcards.

## [0.2.0] - 2023-01-18

### Added

- Possession field in tooth.json to specify directory to remove when uninstalling a tooth.

### Fixed

- Fix failing to fetch tooth when the repository does not contain go.mod file.
- Fix failing to parse tooth file when the tooth is downloaded via GOPROXY.
- Fix failing to parse tooth when tooth.json is the only file in the tooth.

### Changed

- Change extension name of tooth files to .tth

## [0.1.0] - 2023-01-17

### Added

- Basic functions: cache, install, list, show, tooth init, and uninstall.

[unreleased]: https://github.com/LipPkg/Lip/compare/v0.14.2...HEAD
[0.14.2]: https://github.com/LipPkg/Lip/compare/v0.14.1...v0.14.2
[0.14.1]: https://github.com/LipPkg/Lip/compare/v0.14.0...v0.14.1
[0.14.0]: https://github.com/LipPkg/Lip/compare/v0.13.0...v0.14.0
[0.13.0]: https://github.com/LipPkg/Lip/compare/v0.12.0...v0.13.0
[0.12.0]: https://github.com/LipPkg/Lip/compare/v0.11.45141...v0.12.0
[0.11.45141]: https://github.com/LipPkg/Lip/compare/v0.11.4514...v0.11.45141
[0.11.4514]: https://github.com/LipPkg/Lip/compare/v0.11.0...v0.11.4514
[0.11.0]: https://github.com/LipPkg/Lip/compare/v0.10.0...v0.11.0
[0.10.0]: https://github.com/LipPkg/Lip/compare/v0.9.0...v0.10.0
[0.9.0]: https://github.com/LipPkg/Lip/compare/v0.8.3...v0.9.0
[0.8.3]: https://github.com/LipPkg/Lip/compare/v0.8.2...v0.8.3
[0.8.2]: https://github.com/LipPkg/Lip/compare/v0.8.1...v0.8.2
[0.8.1]: https://github.com/LipPkg/Lip/compare/v0.8.0...v0.8.1
[0.8.0]: https://github.com/LipPkg/Lip/compare/v0.7.1...v0.8.0
[0.7.1]: https://github.com/LipPkg/Lip/compare/v0.7.0...v0.7.1
[0.7.0]: https://github.com/LipPkg/Lip/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/LipPkg/Lip/compare/v0.5.1...v0.6.0
[0.5.1]: https://github.com/LipPkg/Lip/compare/v0.4.0...v0.5.1
[0.5.0]: https://github.com/LipPkg/Lip/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/LipPkg/Lip/compare/v0.3.4...v0.4.0
[0.3.4]: https://github.com/LipPkg/Lip/compare/v0.3.3...v0.3.4
[0.3.3]: https://github.com/LipPkg/Lip/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/LipPkg/Lip/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/LipPkg/Lip/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/LipPkg/Lip/compare/v0.2.1...v0.3.0
[0.2.1]: https://github.com/LipPkg/Lip/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/LipPkg/Lip/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/LipPkg/Lip/releases/tag/v0.1.0
