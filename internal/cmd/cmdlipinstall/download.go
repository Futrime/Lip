package cmdlipinstall

import (
	"fmt"
	"net/url"
	"os"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/network"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
	"golang.org/x/mod/module"
)

func downloadFileIfNotCached(ctx *context.Context, downloadURL *url.URL) (path.Path, error) {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "downloadFileIfNotCached",
	})

	cachePath, err := getCachePath(ctx, downloadURL)
	if err != nil {
		return path.Path{}, fmt.Errorf("failed to get cache path of %v\n\t%w", downloadURL, err)
	}

	// Skip downloading if the file is already in the cache.
	if _, err := os.Stat(cachePath.LocalString()); os.IsNotExist(err) {
		log.Infof("Downloading %v", downloadURL)

		var enableProgressBar bool
		if log.GetLevel() == log.PanicLevel || log.GetLevel() == log.FatalLevel ||
			log.GetLevel() == log.ErrorLevel || log.GetLevel() == log.WarnLevel {
			enableProgressBar = false
		} else {
			enableProgressBar = true
		}

		proxyURL, err := ctx.ProxyURL()
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to get proxy URL\n\t%w", err)
		}

		if err := network.DownloadFile(downloadURL, proxyURL, cachePath, enableProgressBar); err != nil {
			return path.Path{}, fmt.Errorf("failed to download file\n\t%w", err)
		}

	} else if err != nil {
		return path.Path{}, fmt.Errorf("failed to check if file exists\n\t%w", err)
	} else {
		debugLogger.Debugf("File %v already exists in the cache, skip downloading", cachePath.LocalString())
	}

	return cachePath, nil
}

// downloadToothArchiveIfNotCached downloads the tooth archive from the Go module proxy
// if it is not cached, and returns the path to the downloaded tooth archive.
func downloadToothArchiveIfNotCached(ctx *context.Context, toothRepoPath string,
	toothVersion semver.Version) (tooth.Archive, error) {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "downloadToothArchiveIfNotCached",
	})

	goModuleProxyURL, err := ctx.GoModuleProxyURL()
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to get Go module proxy URL\n\t%w", err)
	}

	downloadURL, err := network.GenerateGoModuleZipFileURL(toothRepoPath, toothVersion, goModuleProxyURL)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to generate Go module zip file URL\n\t%w", err)
	}

	cachePath, err := downloadFileIfNotCached(ctx, downloadURL)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to download file\n\t%w", err)
	}

	debugLogger.Debugf("Downloaded tooth archive from %v to %v", downloadURL, cachePath.LocalString())

	archive, err := tooth.MakeArchive(cachePath)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to open archive %v\n\t%w", cachePath.LocalString(), err)
	}

	if err := validateToothArchive(archive, toothRepoPath, toothVersion); err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to validate archive\n\t%w", err)
	}

	debugLogger.Debugf("Downloaded tooth archive %v", cachePath.LocalString())

	return archive, nil
}

func downloadToothAssetArchiveIfNotCached(ctx *context.Context, archive tooth.Archive) error {
	metadata := archive.Metadata()
	assetURL, err := metadata.AssetURL()
	if err != nil {
		return fmt.Errorf("failed to get asset URL\n\t%w", err)
	}

	if assetURL.String() == "" {
		return nil
	}

	// Rewrite GitHub URL to GitHub mirror URL if it is set.

	gitHubMirrorURL, err := ctx.GitHubMirrorURL()
	if err != nil {
		return fmt.Errorf("failed to get GitHub mirror URL\n\t%w", err)
	}

	if network.IsGitHubDirectDownloadURL(assetURL) {
		// HTTP or HTTPS URL from GitHub.

		mirroredURL, err := network.GenerateGitHubMirrorURL(assetURL, gitHubMirrorURL)
		if err != nil {
			return fmt.Errorf("failed to generate GitHub mirror URL\n\t%w", err)
		}

		if _, err := downloadFileIfNotCached(ctx, mirroredURL); err != nil {
			return fmt.Errorf("failed to download file\n\t%w", err)
		}

	} else if assetURL.Scheme == "http" || assetURL.Scheme == "https" {
		// Other HTTP or HTTPS URL.

		if _, err := downloadFileIfNotCached(ctx, assetURL); err != nil {
			return fmt.Errorf("failed to download file\n\t%w", err)
		}

	} else if err := module.CheckPath(assetURL.String()); err == nil {
		// Go module path.

		goModuleProxyURL, err := ctx.GoModuleProxyURL()
		if err != nil {
			return fmt.Errorf("failed to get Go module proxy URL\n\t%w", err)
		}

		downloadURL, err := network.GenerateGoModuleZipFileURL(assetURL.String(), archive.Metadata().Version(), goModuleProxyURL)
		if err != nil {
			return fmt.Errorf("failed to generate Go module zip file URL\n\t%w", err)
		}

		if _, err := downloadFileIfNotCached(ctx, downloadURL); err != nil {
			return fmt.Errorf("failed to download file\n\t%w", err)
		}

	} else {
		return fmt.Errorf("unsupported asset URL: %v", assetURL)
	}

	return nil
}

func getCachePath(ctx *context.Context, u *url.URL) (path.Path, error) {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "getCachePath",
	})

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return path.Path{}, fmt.Errorf("failed to get cache directory\n\t%w", err)
	}

	cacheFileName := url.QueryEscape(u.String())
	cachePath := cacheDir.Join(path.MustParse(cacheFileName))

	debugLogger.Debugf("Cache path of %v is %v", u, cachePath.LocalString())

	return cachePath, nil
}
