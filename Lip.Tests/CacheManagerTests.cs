﻿using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Flurl;
using Lip.Context;
using Moq;
using Semver;
using SharpCompress.Archives.Zip;

namespace Lip.Tests;

public class CacheManagerTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");

    [Fact]
    public void CacheSummary_With_Passes()
    {
        // Arrange.
        CacheManager.CacheSummary cacheSummary = new()
        {
            DownloadedFiles = [],
            GitRepos = [],
            PackageManifestFiles = []
        };

        // Act.
        cacheSummary = cacheSummary with { };
    }

    [Fact]
    public async Task Clean_BaseCacheDirExists_Passes()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        await cacheManager.Clean();

        // Assert.
        Assert.False(fileSystem.Directory.Exists(s_cacheDir));
    }

    [Fact]
    public async Task Clean_BaseCacheDirNotExists_Passes()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        await cacheManager.Clean();

        // Assert.
        Assert.False(fileSystem.Directory.Exists(s_cacheDir));
    }

    [Fact]
    public async Task GetDownloadedFile_ValidUrl_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        await using Stream stream = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetDownloadedFIle_WithGitHubProxy_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/github-proxy/user/repo/test.file"),
            Path.Join(
                s_cacheDir,
                "downloaded_files",
                "https%3A%2F%2Fexample.com%2Fgithub-proxy%2Fuser%2Frepo%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(
            context.Object,
            pathManager,
            Url.Parse("https://example.com/github-proxy"));

        Url url = Url.Parse("https://github.com/user/repo/test.file");

        // Act.
        await using Stream stream = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetDownloadedFile_FileExists_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        await using Stream stream = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetDownloadedFile_PathIsDirectory_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        Url url = Url.Parse("https://example.com/test.file");

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetDownloadedFile(url));
    }

    [Fact]
    public async Task GetGitRepoDir_ValidPackageSpecifier_ReturnsDirectoryInfo()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var git = new Mock<IGit>();
        git.Setup(g => g.Clone(
            "https://example.com/repo",
            Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"),
            "v1.0.0",
            1))
            .Callback<string, string, string?, int?>((url, path, branch, depth) =>
            {
                fileSystem.AddDirectory(path);
            });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act.
        IDirectoryInfo directoryInfo = await cacheManager.GetGitRepoDir(packageSpecifier);

        // Assert.
        Assert.NotNull(directoryInfo);
        Assert.True(fileSystem.Directory.Exists(directoryInfo.FullName));
    }

    [Fact]
    public async Task GetGitRepoDir_DirectoryExists_ReturnsDirectoryInfo()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockDirectoryData() }
        });

        var git = new Mock<IGit>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act.
        IDirectoryInfo directoryInfo = await cacheManager.GetGitRepoDir(packageSpecifier);

        // Assert.
        Assert.NotNull(directoryInfo);
        Assert.True(fileSystem.Directory.Exists(directoryInfo.FullName));
    }

    [Fact]
    public async Task GetGitRepoDir_GitClientNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetGitRepoDir(packageSpecifier));
    }

    [Fact]
    public async Task GetGitRepoDir_PathIsFile_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockFileData("test") }
        });

        var git = new Mock<IGit>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetGitRepoDir(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageManifestFile_FileExists_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act.
        await using Stream stream = await cacheManager.GetPackageManifestFile(packageSpecifier);

        // Assert.
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageManifestFile_PathIsDirectory_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetPackageManifestFile(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageManifestFile_NoRemoteSource_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetPackageManifestFile(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageManifestFile_WithGit_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0", "tooth.json"), new("test") }
        });

        var git = new Mock<IGit>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act.
        await using Stream stream = await cacheManager.GetPackageManifestFile(packageSpecifier);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageManifestFile_GitRepoNotContainsManifestFile_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockDirectoryData() }
        });

        var git = new Mock<IGit>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetPackageManifestFile(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageManifestFile_WithGoModuleProxy_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Fgo-mod-proxy%2Fexample.com%2Frepo%2F%40v%2Fv1.0.0.zip"),
                new MockFileData(CreateSampleGoModuleProxyArchive("example.com/repo", SemVersion.Parse("1.0.0"), isEmpty: false))
        }
        });

        var downloader = new Mock<IDownloader>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(
            context.Object,
            pathManager,
            goModuleProxy: Url.Parse("https://example.com/go-mod-proxy"));

        PackageSpecifier packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act.
        await using Stream stream = await cacheManager.GetPackageManifestFile(packageSpecifier);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageManifestFile_GoModuleIncompatibleVersion_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Fgo-mod-proxy%2Fexample.com%2Frepo%2F%40v%2Fv2.0.0%2Bincompatible.zip"),
                new MockFileData(CreateSampleGoModuleProxyArchive("example.com/repo", SemVersion.Parse("2.0.0"), isEmpty: false))
        }
        });

        var downloader = new Mock<IDownloader>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(
            context.Object,
            pathManager,
            goModuleProxy: Url.Parse("https://example.com/go-mod-proxy"));

        PackageSpecifier packageSpecifier = PackageSpecifier.Parse("example.com/repo@2.0.0");

        // Act.
        await using Stream stream = await cacheManager.GetPackageManifestFile(packageSpecifier);

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%402.0.0.json")));
        Assert.NotNull(stream);
        Assert.Equal("test", new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageManifestFile_GoModuleArchiveNotContainsManifestFile_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Fgo-mod-proxy%2Fexample.com%2Frepo%2F%40v%2Fv1.0.0.zip"),
                new MockFileData(CreateSampleGoModuleProxyArchive("example.com/repo", SemVersion.Parse("1.0.0"), isEmpty: true))
        }
        });

        var downloader = new Mock<IDownloader>();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(
            context.Object,
            pathManager,
            goModuleProxy: Url.Parse("https://example.com/go-mod-proxy"));

        PackageSpecifier packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await cacheManager.GetPackageManifestFile(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageManifestFile_GoModuleVersionWithBuildMetadata_ThrowsArgumentException()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, goModuleProxy: Url.Parse("https://example.com/go-mod-proxy"));

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/repo",
            VariantLabel = "",
            Version = SemVersion.Parse("1.0.0+build")
        };

        // Act & assert.
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await cacheManager.GetPackageManifestFile(packageSpecifier));
        Assert.Equal("version", ex.ParamName);
    }

    [Fact]
    public async Task List_NoCacheDirectories_ReturnsEmptyList()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Empty(listResult.GitRepos);
        Assert.Empty(listResult.PackageManifestFiles);
    }

    [Fact]
    public async Task List_DownloadedFileExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Single(listResult.DownloadedFiles);
        Assert.Equal("https://example.com/test.file", listResult.DownloadedFiles.Keys.Single());
        Assert.Equal("test", new StreamReader(listResult.DownloadedFiles.Values.Single().OpenRead()).ReadToEnd());
        Assert.Empty(listResult.GitRepos);
        Assert.Empty(listResult.PackageManifestFiles);
    }

    [Fact]
    public async Task List_GitRepoExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Single(listResult.GitRepos);
        Assert.Equal("https://example.com/repo", listResult.GitRepos.Keys.Single().Url);
        Assert.Equal("v1.0.0", listResult.GitRepos.Keys.Single().Tag);
        Assert.Equal(Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), listResult.GitRepos.Values.Single().FullName);
        Assert.Empty(listResult.PackageManifestFiles);
    }

    [Fact]
    public async Task List_PackageManifestFileExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Empty(listResult.GitRepos);
        Assert.Single(listResult.PackageManifestFiles);
        Assert.Equal("example.com/repo@1.0.0", listResult.PackageManifestFiles.Keys.Single().SpecifierWithoutVariant);
        Assert.Equal("test", new StreamReader(listResult.PackageManifestFiles.Values.Single().OpenRead()).ReadToEnd());
    }

    private static byte[] CreateSampleGoModuleProxyArchive(string goModulePath, SemVersion version, bool isEmpty)
    {
        using MemoryStream contentStream = new(Encoding.UTF8.GetBytes("test"));

        ZipArchive archive = ZipArchive.Create();
        if (!isEmpty)
        {
            archive.AddEntry($"{goModulePath}@v{version}{(version.Major >= 2 ? "+incompatible" : "")}/tooth.json", contentStream);
        }

        using MemoryStream stream = new();
        archive.SaveTo(stream);

        return stream.ToArray();
    }
}
