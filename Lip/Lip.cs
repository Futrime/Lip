﻿using DotNet.Globbing;
using Flurl;
using Lip.Context;

namespace Lip;

/// <summary>
/// The main class of the Lip library.
/// </summary>
public partial class Lip
{
    private readonly CacheManager _cacheManager;
    private readonly IContext _context;
    private readonly PackageManager _packageManager;
    private readonly PathManager _pathManager;
    private readonly RuntimeConfig _runtimeConfig;

    public Lip(RuntimeConfig runtimeConfig, IContext context)
    {
        _context = context;
        _runtimeConfig = runtimeConfig;

        _pathManager = new(context.FileSystem, baseCacheDir: runtimeConfig.Cache, workingDir: context.WorkingDir);

        Url? githubProxyUrl = runtimeConfig.GitHubProxy != string.Empty ? Url.Parse(runtimeConfig.GitHubProxy) : null;
        Url? goModuleProxyUrl = runtimeConfig.GoModuleProxy != string.Empty ? Url.Parse(runtimeConfig.GoModuleProxy) : null;
        _cacheManager = new(_context, _pathManager, githubProxyUrl, goModuleProxyUrl);

        _packageManager = new(_context, _cacheManager, _pathManager);
    }

    private string? GetPlacementRelativePath(PackageManifest.PlaceType placement, string fileSourceEntryKey)
    {
        if (placement.Type == PackageManifest.PlaceType.TypeEnum.File)
        {
            string fileName = _context.FileSystem.Path.GetFileName(fileSourceEntryKey);

            if (fileSourceEntryKey == placement.Src)
            {
                return string.Empty;
            }

            Glob glob = Glob.Parse(placement.Src);

            if (glob.IsMatch(fileSourceEntryKey))
            {
                return fileName;
            }

            return null;
        }
        else if (placement.Type == PackageManifest.PlaceType.TypeEnum.Dir)
        {
            string placementSrc = placement.Src;

            if (!placementSrc.EndsWith('/'))
            {
                placementSrc += '/';
            }

            if (!fileSourceEntryKey.StartsWith(placementSrc))
            {
                return null;
            }

            return fileSourceEntryKey[placementSrc.Length..];
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
