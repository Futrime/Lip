# tooth.json 文件参考

每一个 Lip tooth包都通过tooth.json文件定义，该文件描述tooth包的属性，包括对其他tooth包的依赖信息和其他信息。

这些属性包括：

- **format version** 这一tooth.json的格式版本。

- **tooth** 这一tooth包的路径。这一属性应向Lip提供一个Lip可以下载到这个tooth包的位置，比如一个代码存储库。当这一路径与版本号结合使用时，它可以作为一个唯一标识符。

- **version** 这一tooth包的版本号。

- **dependencies** 这一tooth包所依赖的tooth包以及他们的版本。

- **information** 这一tooth包的信息，包括名称，作者，描述等。

- **placement** 这一tooth包的位置这是一个说明tooth包的文件应当被如何放置到安装目录的列表。

- **possession** 这一tooth包占有的文件。这是一个声明tooth包所占用的文件夹的列表

- **format_version**、**tooth path**和**version**是必须的。其他属性是可选。

你可以通过运行 lip tooth init 命令来生成和初始化一个tooth.json。下面的例子创建了一个 tooth.json 文件。

```shell
lip tooth init
```

## 例子

一个tooth.json文件包括字段，就像下面的样例中的一样。这些内容将在本章节往后的地方被介绍。

```json
{
    "format_version": 1,
    "tooth": "github.com/liteldev/liteloaderbds",
    "version": "2.9.0",
    "dependencies": {
        "test.test/test/depend": [
            [
                ">=1.0.0",
                "<=1.1.0"
            ],
            [
                "2.0.x"
            ]
        ]
    },
    "information": {
        "name": "LiteLoaderBDS",
        "description": "Epoch-making and cross-language Bedrock Dedicated Server plugin loader.",
        "author": "LiteLDev",
        "license": "Modified LGPL-3.0",
        "homepage": "www.litebds.com"
    },
    "placement": [
        {
            "source": "LiteLoader.dll",
            "destination": "LiteLoader.dll"
        }
    ],
    "possession": [
        "plugins/LiteLoader/"
    ]
}
```

## `format_version` - 格式版本

表示 tooth.json 文件的格式，Lip会根据这个字段来解析 tooth.json。

### 样例

```json
{
    "format_version": 1
}
```

### 注意

目前只有 `1` 是有效值。

## `tooth` - 包的路径

tooth包的路径，是tooth包的唯一标示符 (当与版本号结合使用时)。

### 语法

通常的，tooth包的路径应该为不含协议前缀的小写URL形式。（例：github.com/liteldev/liteloaderbds ）

只允许小写字母、数字、破折号、下划线、圆点和斜线[a-z0-9-_./]。大写字母在解析前会被转换为小写字母。

### 样例

```json
{
    "tooth": "example.com/mytooth"
}
```

### 注意

tooth包的路径必须是唯一的。对于大多数的tooth包，这一字段可以是一个Lip能找到你的包的URL。对于永远不会背直接下载的tooth包，tooth的路径可为一些你可以保证唯一性的名称。

注意，tooth的路径不应该包括协议前置（如 "https://" 或者 "git://"），这是不符合语法的。另外tooth的路径不应该以".tth"后缀作为结尾，因为这会导致Lip将其视为一个独立的tooth文件

如果你想发布你的tooth包，你应该让这一字段是一个真正的URL。例如，它应该以小写字母或者数字开头。

## `version` - 版本

### 语法

我们采用[Semantic Versioning 2.0.0](https://semver.org)（语义化版本 2.0.0） 并简化其规则

- 标准的版本号必须（MUST）采用 X.Y.Z 的格式，其中 X、Y 和 Z 为非负的整数，且禁止（MUST NOT）在数字前方补零。（例如: 1.01.02是被禁止的）。X 是主版本号、Y 是次版本号、而 Z 为修订号。每个元素必须（MUST）以数值来递增。例如：1.9.1 -> 1.10.0 -> 1.11.0。

- 标记版本号的tooth包发行后，禁止（MUST NOT）改变该版本软件的内容。任何修改都必须（MUST）以新版本发行。

- 主版本号为零（0.y.z）的软件处于开发初始阶段，一切都可能随时被改变。这样的公共 API 不应该被视为稳定版。当处于早期开发阶段时，请将主版本号设置为0.

- 修订号 Z（x.y.Z | x > 0）必须（MUST）在只做了向下兼容的修正时才递增。这里的修正指的是针对不正确结果而进行的内部修改。

- 次版本号 Y（x.Y.z | x > 0）必须（MUST）在有向下兼容的新功能出现时递增。在任何公共 API 的功能被标记为弃用时也必须（MUST）递增。也可以（MAY）在内部程序有大量新功能或改进被加入时递增，其中可以（MAY）包括修订级别的改变。每当次版本号递增时，修订号必须（MUST）归零。

- 主版本号 X（X.y.z | X > 0）必须（MUST）在有任何不兼容的修改被加入公共 API 时递增。其中可以（MAY）包括次版本号及修订级别的改变。每当主版本号递增时，次版本号和修订号必须（MUST）归零。

- 先行版本号可以（MAY）被标注在修订版之后，先加上一个连接号再加上至多两个句点分隔的标识符来修饰。标识符必须（MUST）由小写字母 [a-z] 组成，而第二个标识符（如果使用）必须（MUST）只包括数字。标识符禁止（MUST NOT）留白。数字型的标识符禁止（MUST NOT）在前方补零。先行版的优先级低于相关联的标准版本。其补丁版本必须（MUST）为零。被标上先行版本号则表示这个版本并非稳定而且可能无法满足预期的兼容性需求。例如：1.0.0-alpha, 1.0.0-alpha.1, 1.2.0-beta。 注意：1.0.1-alpha 是不被允许的。

- 版本的优先层级指的是不同版本在排序时如何比较。 它是根据以下规则来计算的。

  1. 判断优先层级时，必须（MUST）把版本依序拆分为主版本号、次版本号、修订号及先行版本号后进行比较。

  2. 由左到右依序比较每个标识符，第一个差异值用来决定优先层级：主版本号、次版本号及修订号以数值比较。

     例如：1.0.0 < 2.0.0 < 2.1.0 < 2.1.1。

  3. 当主版本号、次版本号及修订号都相同时，改以优先层级比较低的先行版本号决定。

     例如：1.0.0-alpha < 1.0.0。

  4. 有相同主版本号、次版本号及修订号的两个先行版本号，其优先层级必须（MUST）透过由左到右的每个被句点分隔的标识符来比较，直到找到一个差异值后决定：

     1. 只有数字的标识符以数值高低比较。

     2. 有字母或连接号时则逐字以 ASCII 的排序来比较。在比较时，当一个标识符已经结束而另一个标识符没有结束时，则未结束的标识符优先层级较低。
   
     例如： 1.0.0-alph < 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0.

### 样例

生产版本的例子：

```json
{
    "version": "1.2.3"
}
```

预先发布的例子：

```json
{
    "version": "1.2.0-beta.3"
}
```

早期开发版本的例子：

```json
{
    "version": "0.1.2"
}
```

### 注意

在发布你的tooth包时，你应该在Git标签上设置前缀 "v"，例如v1.2.3。否则，Lip将无法正确解析标签。

由于GOPROXY将前缀为 "v0.0.0" 的版本视为伪版本，如果你想发布你的tooth包，你不应该设置以 "0.0.0" 开头的版本。

## `dependencies` - 依赖

### 语法

Lip提供了一些版本匹配规则：

- **1.2.0** 必须与1.2.0完全匹配
- **>1.2.0** 必须大于1.2.0，但主版本不变，如1.3.0、1.4.0等，但不能是2.0.0
- **>=1.2.0** 
- **<1.2.0**
- **<=1.2.0**
- **!1.2.0** 必须不是 1.2.0
- **1.2.x** 将会匹配1.2.0, 1.2.1 等 但是不能是 1.3.0

最外层列表中的所有规则将用OR计算，而嵌套列表中的规则将用AND计算。在`[[">=3.0.5", "<=3.0.7"],["3.0.9"]]`这一例子中，libopenssl3可以匹配3.0.5、3.0.6、3.0.7和3.0.9版本，但不能匹配3.0.8，你可以把它的规则看作是：

```
(>=3.0.5 AND <=3.0.7) OR 3.0.9
```

不允许多级嵌套。

### 样例

```json
{
    "dependencies": {
        "test.test/test/depend": [
            [
                ">=1.0.0",
                "<=1.1.0"
            ],
            [
                "2.0.x"
            ]
        ]
    }
}
```

### 注意

不允许使用小版本通配符，如不能使用1.x.x。

## `information`

声明你的tooth的必要信息，并添加任何你喜欢的信息。

### 语法

这个字段没有语法限制。你可以按照JSON规则写任何东西。

### Examples

```json
{
    "information": {
        "name": "LiteLoaderBDS",
        "description": "Epoch-making and cross-language Bedrock Dedicated Server plugin loader.",
        "author": "LiteLDev",
        "license": "Modified LGPL-3.0",
        "homepage": "www.litebds.com",
        "thanks": "All contributors!"
    }
}
```

### 注意

有些字段是习惯性的，可能会显示在一些的搜索页面上。例如下面列出的这些:


- name: tooth包的名字
- description: 对tooth包的一行描述
- author: 作者名字
- license: tooth包的协议，私有包请留空
- homepage: tooth包的主页

## `placement`

向Lip提供关于如何放置包内文件的信息。安装时，“source”的内容会被放置到“destination”；卸载时，“destination”的内容会被移除。

### 语法

每个放置规则应该包含一个源字段和一个目标字段。Lip将从源字段指定的tooth包中的相对路径中提取文件，并将它们放置到目标地指定的BDS根目录的相对路径中。

如果源目录和目标目录都以 "*"结尾，则该位置将被视为通配符。Lip将递归地把源目录下的所有文件放置到目标目录。

在这里我们做了一个严格的规定，源和目的地只能包含字母、数字、连字符、下划线、点、斜线和星号（对于最后一个字母作为通配符处理）[a-zA-Z0-9-_.\/*]。如果你想把文件放到BDS的根部，你应该在`placement`字段中指定每个文件。第一个字母不应该是斜线或点。最后一个字母不应该是斜线。

### 样例

从特定的文件夹中提取并放置到特定的文件夹中：

```json
{
    "placement": [
        {
            "source": "build",
            "destination": "plugins"
        },
        {
            "source": "assets",
            "destination": "plugins/myplugin"
        }
    ]
}
```

### 注意

不要添加任何前缀，如 "/", "./" 或 "../".否则，Lip将拒绝安装这一tooth包。

## `possession`

声明哪些文件夹是由tooth包拥有的。卸载时，声明的文件夹中的文件将被删除。升级或重新安装时，在新旧两个版本的possession中都制定了的文件夹中的文件不会被移除（placement指定的除外）。

### 语法

列表中的每一项都应该是相对于BDS根的有效目录路径，以"/"结尾。

### 样例

```json
{
    "possession": [
        "plugins/LiteLoader/"
    ]
}
```

### 注意

不要占有任何可能被其他tooth包使用的目录，例如像`worlds/`这样的公共目录。

## 语法

下列JSON Schema展示了一个完整的tooth包的JSON文件的语法。

```json
{
    "$schema": "https://json-schema.org/draft/2020-12/schema",
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
        }
    }
}
```