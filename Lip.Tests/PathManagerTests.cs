﻿using System.IO.Abstractions.TestingHelpers;
using Flurl;
using Semver;

namespace Lip.Tests;

public class PathManagerTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");
    private static readonly string s_workingDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "current", "dir")
        : Path.Join("/", "current", "dir");

    [Fact]
    public void GitRepoInfo_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PathManager.GitRepoInfo repoInfo = new()
        {
            Url = "https://example.com/repo",
            Tag = "main",
        };

        // Act.
        repoInfo = repoInfo with { };

        // Assert.
        // No need to assert anything.
    }

    [Fact]
    public void GetBaseCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act & assert.
        Assert.Throws<InvalidOperationException>(() => pathManager.BaseCacheDir);
    }

    [Fact]
    public void GetBaseCacheDir_WithBaseCacheDir_ReturnsFullPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseCacheDir = pathManager.BaseCacheDir;

        // Assert.
        Assert.Equal(s_cacheDir, baseCacheDir);
    }

    [Fact]
    public void GetBaseDownloadedFileCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        PathManager pathManager = new(fileSystem);

        // Act & assert.
        Assert.Throws<InvalidOperationException>(() => pathManager.BaseDownloadedFileCacheDir);
    }

    [Fact]
    public void GetBaseDownloadedFileCacheDir_WithBaseCacheDir_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() },
        });

        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseAssetCacheDir = pathManager.BaseDownloadedFileCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "downloaded_files"), baseAssetCacheDir);
    }

    [Fact]
    public void GetBaseGitRepoCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act & assert.
        Assert.Throws<InvalidOperationException>(() => pathManager.BaseGitRepoCacheDir);
    }

    [Fact]
    public void GetBaseGitRepoCacheDir_WithBaseCacheDir_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() },
        });

        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseGitRepoCacheDir = pathManager.BaseGitRepoCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "git_repos"), baseGitRepoCacheDir);
    }

    [Fact]
    public void GetCurrentPackageManifestPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestPath = pathManager.CurrentPackageManifestPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth.json"), manifestPath);
    }

    [Fact]
    public void GetCurrentPackageLockPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string recordPath = pathManager.CurrentPackageLockPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth_lock.json"), recordPath);
    }

    [Fact]
    public void GetPackageManifestFileName_WhenCalled_ReturnsCorrectFileName()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestFileName = pathManager.PackageManifestFileName;

        // Assert.
        Assert.Equal("tooth.json", manifestFileName);
    }

    [Fact]
    public void GetRuntimeConfigPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        string runtimeConfigPath = pathManager.RuntimeConfigPath;

        // Assert.
        Assert.Equal(
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json"),
            runtimeConfigPath);
    }

    [Fact]
    public void GetWorkingDir_WhenCalled_ReturnsCurrentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string workingDir = pathManager.WorkingDir;

        // Assert.
        Assert.Equal(s_workingDir, workingDir);
    }

    [Fact]
    public void GetWorkingDir_WorkingDirProvided_ReturnsWorkingDir()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, workingDir: s_workingDir);

        // Act.
        string workingDir = pathManager.WorkingDir;

        // Assert.
        Assert.Equal(s_workingDir, workingDir);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1")]
    [InlineData("https://example.com/path/to/asset", "https%3A%2F%2Fexample.com%2Fpath%2Fto%2Fasset")]
    [InlineData("https://example.com/", "https%3A%2F%2Fexample.com%2F")]
    public void GetDownloadedFileCachePath_ValidUrls_ReturnsEscapedPath(string url, string expectedFileName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string cachePath = pathManager.GetDownloadedFileCachePath(Url.Parse(url));

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "downloaded_files", expectedFileName),
            cachePath);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "v1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1", "v1")]
    [InlineData("https://github.com/dotnet/runtime", "main", "https%3A%2F%2Fgithub.com%2Fdotnet%2Fruntime", "main")]
    [InlineData("https://github.com/dotnet/aspnetcore", "release/6.0", "https%3A%2F%2Fgithub.com%2Fdotnet%2Faspnetcore", "release%2F6.0")]
    [InlineData("https://github.com/microsoft/vscode", "1.60.0", "https%3A%2F%2Fgithub.com%2Fmicrosoft%2Fvscode", "1.60.0")]
    [InlineData("https://github.com/torvalds/linux", "v5.14", "https%3A%2F%2Fgithub.com%2Ftorvalds%2Flinux", "v5.14")]
    [InlineData("https://github.com/apple/swift", "swift-5.5-branch", "https%3A%2F%2Fgithub.com%2Fapple%2Fswift", "swift-5.5-branch")]
    public void GetGitRepoDirCachePath_ValidGitRepos_ReturnsEscapedPath(
        string repoUrl,
        string tag,
        string expectedRepoDir,
        string expectedTagDir)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string repoCacheDir = pathManager.GetGitRepoDirCachePath(new()
        {
            Url = repoUrl,
            Tag = tag,
        });

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "git_repos", expectedRepoDir, expectedTagDir),
            repoCacheDir);
    }

    [Theory]
    [InlineData("1.0.0", "example.com/pkg@v1.0.0/path/to/file")]
    [InlineData("2.0.0", "example.com/pkg@v2.0.0+incompatible/path/to/file")]
    public void GetGoModuleArchiveEntryKey_ValidPackageSpecifier_ReturnsCorrectKey(string version, string expectedKey)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "",
            Version = SemVersion.Parse(version),
        };

        // Act.
        string key = pathManager.GetGoModuleArchiveEntryKey(packageSpecifier, "path/to/file");

        // Assert.
        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public void GetPackageManifestPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestPath = pathManager.GetPackageManifestPath(s_workingDir);

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth.json"), manifestPath);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1")]
    [InlineData("https://example.com/path/to/asset", "https%3A%2F%2Fexample.com%2Fpath%2Fto%2Fasset")]
    [InlineData("https://example.com/", "https%3A%2F%2Fexample.com%2F")]
    public void ParseDownloadedFileCachePath_WhenCalled_ReturnsCorrectUrl(string expectedUrl, string relativePath)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);
        string downloadedFileCachePath = Path.Join(s_cacheDir, "downloaded_files", relativePath);

        // Act.
        Url url = pathManager.ParseDownloadedFileCachePath(downloadedFileCachePath);

        // Assert.
        Assert.Equal(expectedUrl, url);
    }

    [Fact]
    public void ParseDownloadedFileCachePath_InvalidPath_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);
        string downloadedFileCachePath = Path.Join(s_cacheDir, "invalid", "path");

        // Act & assert.
        Assert.Throws<InvalidOperationException>(() => pathManager.ParseDownloadedFileCachePath(downloadedFileCachePath));
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "v1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1", "v1")]
    [InlineData("https://github.com/dotnet/runtime", "main", "https%3A%2F%2Fgithub.com%2Fdotnet%2Fruntime", "main")]
    [InlineData("https://github.com/dotnet/aspnetcore", "release/6.0", "https%3A%2F%2Fgithub.com%2Fdotnet%2Faspnetcore", "release%2F6.0")]
    [InlineData("https://github.com/microsoft/vscode", "1.60.0", "https%3A%2F%2Fgithub.com%2Fmicrosoft%2Fvscode", "1.60.0")]
    [InlineData("https://github.com/torvalds/linux", "v5.14", "https%3A%2F%2Fgithub.com%2Ftorvalds%2Flinux", "v5.14")]
    [InlineData("https://github.com/apple/swift", "swift-5.5-branch", "https%3A%2F%2Fgithub.com%2Fapple%2Fswift", "swift-5.5-branch")]
    public void ParseGitRepoDirCachePath_WhenCalled_ReturnsCorrectRepoInfo(
        string expectedUrl,
        string expectedTag,
        string repoDir,
        string tagDir)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);
        string gitRepoDirCachePath = Path.Join(s_cacheDir, "git_repos", repoDir, tagDir);

        // Act.
        PathManager.GitRepoInfo repoInfo = pathManager.ParseGitRepoDirCachePath(gitRepoDirCachePath);

        // Assert.
        Assert.Equal(expectedUrl, repoInfo.Url);
        Assert.Equal(expectedTag, repoInfo.Tag);
    }

    [Fact]
    public void ParseGitRepoDirCachePath_InvalidPath_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);
        string gitRepoDirCachePath = Path.Join(s_cacheDir, "invalid", "path");

        // Act & assert.
        Assert.Throws<InvalidOperationException>(() => pathManager.ParseGitRepoDirCachePath(gitRepoDirCachePath));
    }
}
