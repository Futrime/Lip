package context

import (
	"encoding/json"
	"fmt"
	"net/url"
	"os"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/path"
)

// Context is the context of the application.
type Context struct {
	config     Config
	lipVersion semver.Version
}

// New creates a new context.
func New(config Config, version semver.Version) *Context {
	return &Context{
		config:     config,
		lipVersion: version,
	}
}

// Config returns the config.
func (ctx *Context) Config() *Config {
	return &ctx.config
}

// GitHubMirrorURL returns the GitHub mirror URL.
func (ctx *Context) GitHubMirrorURL() (*url.URL, error) {
	gitHubMirrorURL, err := url.Parse(ctx.config.GitHubMirrorURL)
	if err != nil {
		return nil, fmt.Errorf("cannot parse GitHub mirror URL\n\t%w", err)
	}

	return gitHubMirrorURL, nil
}

// GoModuleProxyURL returns the go module proxy URL.
func (ctx *Context) GoModuleProxyURL() (*url.URL, error) {
	goModuleProxyURL, err := url.Parse(ctx.config.GoModuleProxyURL)
	if err != nil {
		return nil, fmt.Errorf("cannot parse go module proxy URL\n\t%w", err)
	}

	return goModuleProxyURL, nil
}

// ProxyURL returns the proxy URL.
func (ctx *Context) ProxyURL() (*url.URL, error) {
	proxyURL, err := url.Parse(ctx.config.ProxyURL)
	if err != nil {
		return nil, fmt.Errorf("cannot parse proxy URL\n\t%w", err)
	}

	return proxyURL, nil
}

// LipVersion returns the lip version.
func (ctx *Context) LipVersion() semver.Version {
	return ctx.lipVersion
}

// GlobalDotLipDir returns the global .lip directory.
func (ctx *Context) GlobalDotLipDir() (path.Path, error) {

	userHomeDirStr, err := os.UserHomeDir()
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot get user home directory\n\t%w", err)
	}

	userHomeDir, err := path.Parse(userHomeDirStr)
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot parse user home directory\n\t%w", err)
	}

	globalDotLipDir := userHomeDir.Join(path.MustParse(".lip"))

	return globalDotLipDir, nil
}

// LocalDotLipDir returns the local .lip directory.
func (ctx *Context) LocalDotLipDir() (path.Path, error) {

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot get workspace directory\n\t%w", err)
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot parse workspace directory\n\t%w", err)
	}

	path := workspaceDir.Join(path.MustParse(".lip"))

	return path, nil
}

// CacheDir returns the cache directory.
func (ctx *Context) CacheDir() (path.Path, error) {

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot get global .lip directory\n\t%w", err)
	}

	path := globalDotLipDir.Join(path.MustParse("cache"))

	return path, nil
}

// MetadataDir returns the metadata directory.
func (ctx *Context) MetadataDir() (path.Path, error) {

	localDotLipDir, err := ctx.LocalDotLipDir()
	if err != nil {
		return path.Path{}, fmt.Errorf("cannot get local .lip directory\n\t%w", err)
	}

	path := localDotLipDir.Join(path.MustParse("metadata"))

	return path, nil
}

// CreateDirStructure creates the directory structure.
func (ctx *Context) CreateDirStructure() error {

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get global .lip directory\n\t%w", err)
	}

	if err := os.MkdirAll(globalDotLipDir.LocalString(), 0755); err != nil {
		return fmt.Errorf("cannot create global .lip directory\n\t%w", err)
	}

	localDotLipDir, err := ctx.LocalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get local .lip directory\n\t%w", err)
	}

	if err := os.MkdirAll(localDotLipDir.LocalString(), 0755); err != nil {
		return fmt.Errorf("cannot create local .lip directory\n\t%w", err)
	}

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return fmt.Errorf("cannot get cache directory\n\t%w", err)
	}

	if err := os.MkdirAll(cacheDir.LocalString(), 0755); err != nil {
		return fmt.Errorf("cannot create cache directory\n\t%w", err)
	}

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("cannot get metadata directory\n\t%w", err)
	}

	if err := os.MkdirAll(metadataDir.LocalString(), 0755); err != nil {
		return fmt.Errorf("cannot create metadata directory\n\t%w", err)
	}

	return nil
}

// LoadOrCreateConfigFile loads or creates the config file.
func (ctx *Context) LoadOrCreateConfigFile() error {

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get global .lip directory\n\t%w", err)
	}

	configFilePath := globalDotLipDir.Join(path.MustParse("config.json"))

	if _, err := os.Stat(configFilePath.LocalString()); os.IsNotExist(err) {
		ctx.SaveConfigFile()

	} else if err != nil {
		return fmt.Errorf("cannot get config file info\n\t%w", err)

	} else {
		jsonBytes, err := os.ReadFile(configFilePath.LocalString())
		if err != nil {
			return fmt.Errorf("cannot read config file at %v\n\t%w", configFilePath.LocalString(), err)
		}

		if err := json.Unmarshal(jsonBytes, &ctx.config); err != nil {
			return fmt.Errorf("cannot unmarshal config at %v\n\t%w", configFilePath.LocalString(), err)
		}
	}

	return nil
}

// SaveConfigFile saves the config file.
func (ctx *Context) SaveConfigFile() error {

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get global .lip directory\n\t%w", err)
	}

	configFilePath := globalDotLipDir.Join(path.MustParse("config.json"))

	jsonBytes, err := json.MarshalIndent(ctx.config, "", "  ")
	if err != nil {
		return fmt.Errorf("cannot marshal config\n\t%w", err)
	}

	if err := os.WriteFile(configFilePath.LocalString(), jsonBytes, 0644); err != nil {
		return fmt.Errorf("cannot write config file\n\t%w", err)
	}

	return nil
}
