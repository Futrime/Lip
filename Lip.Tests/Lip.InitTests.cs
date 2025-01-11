﻿using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lip.Tests;

public partial class LipTests
{
    public static readonly string WorkspacePath = OperatingSystem.IsWindows() ? Path.Join("C:", "path", "to", "workspace") : Path.Join("/", "path", "to", "workspace");

    [Fact]
    public async Task Init_Interactive_Passes()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();
        userInteraction.Setup(u => u.PromptForInput(
            "Enter the tooth path (e.g. {DefaultTooth}):",
            "example.com/org/package").Result)
            .Returns("example.com/org/package");
        userInteraction.Setup(u => u.PromptForInput("Enter the package version (e.g. {DefaultVersion}):", "0.1.0").Result)
            .Returns("0.1.0");
        userInteraction.Setup(u => u.PromptForInput("Enter the package name:").Result)
            .Returns("Example Package");
        userInteraction.Setup(u => u.PromptForInput("Enter the package description:").Result)
            .Returns("An example package.");
        userInteraction.Setup(u => u.PromptForInput("Enter the package author:").Result)
            .Returns("Example Author");
        userInteraction.Setup(u => u.PromptForInput("Enter the author's avatar URL:").Result)
            .Returns("https://example.com/avatar.png");
        userInteraction.Setup(u => u.Confirm("Do you want to create the following package manifest file?\n{jsonString}", It.IsAny<string>()).Result)
            .Returns(true);

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new();

        // Act.
        await lip.Init(args);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(WorkspacePath, "tooth.json")));
        Assert.Equal(
            """
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/org/package",
                "version": "0.1.0",
                "info": {
                    "name": "Example Package",
                    "description": "An example package.",
                    "author": "Example Author",
                    "avatar_url": "https://example.com/avatar.png"
                }
            }
            """.ReplaceLineEndings(),
            fileSystem.File.ReadAllText(Path.Join(WorkspacePath, "tooth.json")).ReplaceLineEndings());
    }

    [Fact]
    public async Task Init_WithDefaultValues_Passes()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new()
        {
            Yes = true,
        };

        // Act.
        await lip.Init(args);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(WorkspacePath, "tooth.json")));
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/org/package",
                "version": "0.1.0",
                "info": {}
            }
            """.ReplaceLineEndings(),
            fileSystem.File.ReadAllText(Path.Join(WorkspacePath, "tooth.json")).ReplaceLineEndings());
    }

    [Fact]
    public async Task Init_WithInitialValues_Passes()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new()
        {
            InitAuthor = "Example Author",
            InitAvatarUrl = "https://example.com/avatar.png",
            InitDescription = "An example package.",
            InitName = "Example Package",
            InitTooth = "example.com/org/package",
            InitVersion = "0.1.0",
            Yes = true,
        };

        // Act.
        await lip.Init(args);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(WorkspacePath, "tooth.json")));
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/org/package",
                "version": "0.1.0",
                "info": {
                    "name": "Example Package",
                    "description": "An example package.",
                    "author": "Example Author",
                    "avatar_url": "https://example.com/avatar.png"
                }
            }
            """.ReplaceLineEndings(),
            fileSystem.File.ReadAllText(Path.Join(WorkspacePath, "tooth.json")).ReplaceLineEndings());
    }

    [Fact]
    public async Task Init_OperationCanceled_ThrowsOperationCanceledException()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();
        userInteraction.Setup(u => u.Confirm("Do you want to create the following package manifest file?\n{jsonString}", It.IsAny<string>()).Result)
            .Returns(false);

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new()
        {
            InitAuthor = "Example Author",
            InitAvatarUrl = "https://example.com/avatar.png",
            InitDescription = "An example package.",
            InitName = "Example Package",
            InitTooth = "example.com/org/package",
            InitVersion = "0.1.0",
        };

        // Act and assert.
        await Assert.ThrowsAsync<OperationCanceledException>(() => lip.Init(args));
    }

    [Fact]
    public async Task Init_ManifestFileExists_ThrowsInvalidOperationException()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
            { Path.Join(WorkspacePath, "tooth.json"), new MockFileData("content") },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();
        userInteraction.Setup(u => u.Confirm("Do you want to create the following package manifest file?\n{jsonString}", It.IsAny<string>()).Result)
            .Returns(false);

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new()
        {
            InitAuthor = "Example Author",
            InitAvatarUrl = "https://example.com/avatar.png",
            InitDescription = "An example package.",
            InitName = "Example Package",
            InitTooth = "example.com/org/package",
            InitVersion = "0.1.0",
            Yes = true,
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.Init(args));
    }

    [Fact]
    public async Task Init_OverwritesManifestFile_Passes()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { WorkspacePath, new MockDirectoryData() },
            { Path.Join(WorkspacePath, "tooth.json"), new MockFileData("content") },
        }, currentDirectory: WorkspacePath);

        Mock<ILogger> logger = new();

        Mock<IUserInteraction> userInteraction = new();
        userInteraction.Setup(u => u.Confirm("Do you want to create the following package manifest file?\n{jsonString}", It.IsAny<string>()).Result)
            .Returns(false);

        Lip lip = new(new(), fileSystem, logger.Object, userInteraction.Object);

        Lip.InitArgs args = new()
        {
            Force = true,
            InitAuthor = "Example Author",
            InitAvatarUrl = "https://example.com/avatar.png",
            InitDescription = "An example package.",
            InitName = "Example Package",
            InitTooth = "example.com/org/package",
            InitVersion = "0.1.0",
            Yes = true,
        };

        // Act.
        await lip.Init(args);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(WorkspacePath, "tooth.json")));
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/org/package",
                "version": "0.1.0",
                "info": {
                    "name": "Example Package",
                    "description": "An example package.",
                    "author": "Example Author",
                    "avatar_url": "https://example.com/avatar.png"
                }
            }
            """.ReplaceLineEndings(),
            fileSystem.File.ReadAllText(Path.Join(WorkspacePath, "tooth.json")).ReplaceLineEndings());
    }
}
