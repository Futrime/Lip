# tooth.json File Reference

Each tooth is defined by a tooth.json file that describes the tooth's properties, including its dependencies on other teeth and other information.

You can generate a tooth.json file by running the lip tooth init command. The following example creates a tooth.json file:

```shell
lip tooth init
```

## Schema

Refer to <https://github.com/lippkg/lip/blob/main/schemas/tooth.v2.schema.json>.

## Example

A tooth.json includes directives as shown in the following example. These are described elsewhere in this topic.

```json
{
    "format_version": 2,
    "tooth": "github.com/tooth-hub/example",
    "version": "1.0.0",
    "info": {
        "name": "Example",
        "description": "An example package",
        "author": "exmaple",
        "tags": [
            "example"
        ],
        "avatar_url": "avatar.png"
    },
    "asset_url": "https://github.com/tooth-hub/example/releases/download/v1.0.0/example-1.0.0.zip",
    "commands": {
        "pre_install": [
            "echo \"pre_install\""
        ],
        "post_install": [
            "echo \"post_install\""
        ],
        "pre_uninstall": [
            "echo \"pre_uninstall\""
        ],
        "post_uninstall": [
            "echo \"post_uninstall\""
        ]
    },
    "dependencies": {
        "github.com/tooth-hub/example-deps": ">=1.0.0 <2.0.0 || >=3.0.0 <4.0.0"
    },
    "prerequisites": {
        "github.com/tooth-hub/example-pre": ">=1.0.0"
    },
    "files": {
        "place": [
            {
                "src": "example.js",
                "dest": "dir/example.js"
            },
            {
                "src": "plug/*",
                "dest": "dir/plug/"
            }
        ],
        "preserve": [
            "dir/data.json"
        ],
        "remove": [
            "dir/temp.txt"
        ]
    }
}
```

## `format_version` (required)

Indicates the format of the tooth.json file. lip will parse tooth.json according to this field.

### Examples

```json
{
    "format_version": 2
}
```

### Notes

You should set the format_version to 2.

## `tooth` (required)

Declares the tooth's tooth repository path, which is the tooth's unique identifier (when combined with the tooth version number).

### Syntax

Generally, tooth path should be in the form of a URL without protocol prefix (e.g. github.com/tooth-hub/corepack).

Only letters, digits, dashes, underlines, dots and slashes [A-Za-z0-9-_./] are allowed. Must be identical to the tooth repository path.

### Examples

```json
{
    "tooth": "github.com/tooth-hub/mytooth"
}
```

### Notes

The tooth path must uniquely identify your tooth. For most teeth, the path is a URL where lip can find the code. For teeth that won’t ever be downloaded directly, the tooth path can be just some name you control that will ensure uniqueness.

Note that the tooth path should not include protocol prefix (e.g. "https://" or "git://"), which already violates the syntax.

If you would like to publish your tooth, please make the tooth path a real URL. For example, the first character should be a letter or a digit.

## `version` (required)

### Syntax

We adopted [Semantic Versioning 2.0.0](https://semver.org).

### Examples

Example of a production release:

```json
{
    "version": "1.2.3"
}
```

Example of a pre-release:

```json
{
    "version": "1.2.0-beta.3"
}
```

Example of a early development release:

```json
{
    "version": "0.1.2"
}
```

### Notes

When releasing your tooth, you should set the Git tag with prefix "v", e.g. v1.2.3. Otherwise, lip will not correctly parse the tags.

Since GOPROXY regards versions with prefix "v0.0.0" as psuedo-versions, you should not set the version beginning with "0.0.0" if you would like to publish your tooth.

## `info` (required)

Declares necessary information of your tooth.

### Syntax

Provide information about your tooth in the form of a JSON object with the following fields:

- `name`: (required) the name of your tooth.
- `description`: (required) a short description of your tooth.
- `author`: (required) the author of your tooth.
- `tags`: (required) an array of tags of your tooth. Only [a-z0-9-:] are allowed.
- `avatar_url`: the URL of the tooth's avatar. If not set, the default avatar will be used. If a relative path is provided, it will be regarded as a path relative to **the source repository path**.

!!!tip
    tags shouldn't contain upper letters

### Examples

```json
{
    "info": {
        "name": "Example",
        "description": "An example package",
        "author": "example",
        "tags": [
            "example"
        ],
        "avartar_url": ""
    }
}
```

### Notes

Several tags have special meanings:

Platforms:

- `bds`: indicates that the tooth should be installed on Minecraft Bedrock Dedicated Server platform.
- `levilamina`: indicates that the tooth should be installed on LeviLamina platform.
- `lse`: indicates that the tooth should be installed on LegacyScriptEngine platform.
- `pnx`: indicates that the tooth should be installed on PowerNukkitX platform.

Types:

- `addon`: indicates that the tooth is an addon.
- `library`: indicates that the tooth is a library.
- `plugin`: indicates that the tooth is a plugin.
- `plugin-engine`: indicates that the tooth is a plugin engine.
- `utility`: indicates that the tooth is a utility.
- `world`: indicates that the tooth is a world.

These tags will be used to filter teeth when searching.

## `asset_url` (optional)

Declares the URL of the tooth asset. If this field is set, lip will download the asset and use files in the asset archive instead of files in the tooth repository. This helps when releasing large binary files.

### Syntax

The URL should be a direct link to the asset file or Go Module URL. The asset file should be a zip archive file.  
In lip 0.23.0 and above, you can use `$(version)` to refer to the `version` field above.

### Examples

```json
{
    "asset_url": "https://github.com/tooth-hub/example/releases/download/v1.0.0/example-1.0.0.zip"
}
```

### Notes

For GitHub links, the configured GitHub mirror will be used to download the asset. If the mirror is not configured, the official GitHub will be used.

## `commands` (optional)

Declare commands to run before or after installing or uninstalling the tooth.

### Syntax

This field contains four sub-fields:

- `pre-install`: an array of commands to run before installing the tooth. (optional)
- `post-install`: an array of commands to run after installing the tooth. (optional)
- `pre-uninstall`: an array of commands to run before uninstalling the tooth. (optional)
- `post-uninstall`: an array of commands to run after uninstalling the tooth. (optional)

Each item in the array is a string of the command to run. The command will be run in the workspace.

### Examples

```json
{
    "commands": {
        "pre-install": [
            "echo Pre-install command"
        ],
        "post-install": [
            "echo Post-install command"
        ],
        "pre-uninstall": [
            "echo Pre-uninstall command"
        ],
        "post-uninstall": [
            "echo Post-uninstall command"
        ]
    }
}
```

## `dependencies` (optional)

Declare dependencies of your tooth.

### Syntax

Refer to [here](https://github.com/blang/semver#ranges) for the syntax of version ranges.

### Examples

```json
{
    "dependencies": {
        "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
    }
}
```

## `prerequisites` (optional)

Declare prerequisites of your tooth. The syntax follows the `dependencies` field. The key difference is that prerequisites will not be installed by lip automatically.

### Notes

Some teeth should not be installed automatically, e.g. bds. Automatically installing these teeth may cause severe imcompatibility issues.

## `files` (optional)

Describe how the files in your tooth should be handled.

### Syntax

This field contains three sub-fields:

- `place`: an array to specify how files in the tooth should be place to the workspace. Each item is an object with three sub-fields: (optional)
  - `src`: the source path of the file. It can be a file or a directory with suffix "*" (e.g. `plug/*`). (required)
  - `dest`: the destination path of the file. It can be a file or a directory. If `src` has suffix "*", `dest` must be a directory. Otherwise, `dest` must be a file. (required)
- `preserve`: an array to specify which files in `place` field should be preserved when uninstalling the tooth. Each item is a string of the path of the file. (optional)
- `remove`: an array to specify which files should be removed when uninstalling the tooth. Each item is a string of the path of the file. (optional)

### Examples

```json
{
    "files": {
        "place": [
            {
                "src": "plug/*",
                "dest": "plugins"
            },
            {
                "src": "config.yml",
                "dest": "config.yml"
            }
        ],
        "preserve": [
            "config.yml"
        ],
        "remove": [
            "plugins/ExamplePlugin.dll"
        ]
    }
}
```

### Notes

- Files specified in `place` but not in `preserve` will be removed when uninstalling the tooth. Therefore, you don't need to specify them in `remove`.
- `remove` field is prior to `preserve` field. If a file is specified in both fields, it will be removed.
- Only `place` filed support "*" suffix. `preserve` and `remove` fields do not support it.

## `platforms` (optional)

Declare platform-specific configurations.

### Syntax

This field is an array of platform-specific configurations. Each item is an object with these sub-fields:

- `asset_url`: same as `asset_url` field. (optional)
- `commands`: same as `commands` field. (optional)
- `dependencies`: same as `dependencies` field. (optional)
- `prerequisites`: same as `prerequisites` field. (optional)
- `files`: same as `files` field. (optional)
- `goos`: the target operating system. For the values, see [here](https://go.dev/doc/install/source#environment). (required)
- `goarch`: the target architecture. For the values, see [here](https://go.dev/doc/install/source#environment). Omitting means match all. (optional)

If provided and matched, the platform-specific configuration will override the global configuration.

### Examples

```json
{
    "platforms": [
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Windows"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "windows"
        },
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Linux AMD64"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "linux",
            "goarch": "amd64"
        }
    ]
}
```

### Notes

If multiple platform-specific configurations are matched, the last one will override the previous ones. Therefore, you should put the most specific configuration at the end of the array.

If a platform-specific configuration is set, `commands`, `dependencies` and `files` in the global configuration will be ignored, no matter whether they are set or not in the platform-specific configuration. Thus, it is highly recommended not to set any of them in the global configuration if you would like to set platform-specific configurations.
