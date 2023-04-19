## Plugins System Components

The **Plugins System** is a modular and extensible architecture that provides a flexible way to manage plugins in an application. It consists of several key components, such as the `IPluginsInstaller`, `IPluginsDiscoveryOrchestrator`, `IPluginsSystemResourceLoader<T>`, `IRepository<PluginSource>`, `IObsoletePluginsRemover`, and `IInstalledPluginLoader`, that work together to enable the discovery, installation, uninstallation and loading of plugins at runtime. The composition-based design promotes the use of composition over inheritance, allowing for extensible plugin management through the use of loosely-coupled components that can be easily replaced or extended without modifying the core logic of the system. This documentation provides an overview of the key components of the **Plugins System** and its functionalities.

### Components of the Plugins System
The Plugin System consists of the following components:

1. **IPluginsInstaller**

    The `IPluginsInstaller` interface is responsible for installing plugins, uninstalling plugins and listing installed plugins. It supports installing from various sources given that a **plugin package** stream is provided. 

2. **IPluginsDiscoveryOrchestrator**

    The `IPluginsDiscoveryOrchestrator` interface is responsible for orchestrating the discovery process of plugins. This component interacts with various **plugin source**s and different discovery mechanisms to discover available plugins.

3. **IPluginsSystemResourceLoader\<T\>**

    The `IPluginsSystemResourceLoader<T>` interface is responsible for loading resources required by the plugin system, such as assemblies, configuration files, or other resources, from different sources. The T type parameter represents the type of resources to be loaded, such as `IPluginDiscovererProvider` or `IPluginFetcher`.
    
    - **IPluginDiscovererProvider** represents a plugin system resource that creates `IPluginDiscoverer` for supported plugin sources.
        - **IPluginDiscover** represents a component that discovers available plugins from a *single* plugin source.

    - **IPluginFetcher** represents a plugin system resource that is capable of fetching the plugin package from a discovered plugin. 
    
    For the `IPluginsDiscoveryOrchestrator` to discover plugins, at least one `IPluginDiscovererProvider` and the supporting `IPluginFetcher`(s) need to be loaded into the system.
    
4. **IRepository\<PluginSource\>**

    The `IRepository<PluginSource>` interface is responsible for managing plugin sources provided by a plugin consumer. It supports adding and removing sources as well as notifying these changes.

5. **IObsoletePluginsRemover**

    The `IObsoletePluginsRemover` is responsible for determining which plugins are considered obsolete and removing the binaries from the application.

6. **IInstalledPluginLoader**

    The `IInstalledPluginLoader` interface is responsible for loading installed plugins into the application.


### Usage

#### Construct a Plugins System
Creating a **Plugins System** requires all the components mentioned above provided. A client can either provide their custom implementation or use the default file system-based implementation provided by `Microsoft.Performance.Toolkit.Plugins.Runtime`. A file system directory is required which will be the root directory of the installed plugins. Additionally, a plugin loading callback and a logger factory need to be provided.

```CSharp
using Microsoft.Performance.Toolkit.Plugins.Runtime

var ps = PluginsSystem.CreateFileBasedPluginsSystem(
    @"\InstallationRoot",
    (pluginDir) => pluginsLoader.LoadPlugin(pluginDir),
    (t) => Logger.Create(t));
```

#### Discover Plugins
1. Add a plugin source

```CSharp
ps.PluginSourceRepository.Add(new PluginSource(new Uri(@"http://fakeazure.com/1")));
```

2. Add a discoverer provider and a fetcher
```CSharp
ps.DiscovererProviderResourceLoader.Add(new AzurePluginDiscovererProvider());
ps.FetcherResourceLoader.Add(new AzurePluginFetcher());
```

3. Discover all available plugins in their latest version from `"http://fakeazure.com/1"`
```CSharp
var plugins_latest = await ps.Discoverer.GetAvailablePluginsLatestAsync(CancellationToken.None);
```

4. Discover all versions of a particular plugin from `"http://fakeazure.com/1"`
```CSharp
var plugins_allVersions = await ps.Discoverer.GetAllVersionsOfPluginAsync(
    plugins_latest.First().Info.Identity, CancellationToken.None);
```

### Install and Uninstall Plugins
1. Install a discovered plugin
```CSharp
await ps.InstallAvailablePlugin(
    plugins_latest.First(),
    CancellationToken.None,
    null,
    Logger.Create<PluginsSystem>());
```

2. Install a local plugin package
```CSharp
using var stream = File.OpenRead(pluginPackagePath);
var installedPlugin = await ps.Installer.InstallPluginAsync(
    stream,
    new Uri(pluginPackagePath),
    CancellationToken.None,
    null
)
```
3. Uninstall a plugin
```CSharp
await ps.Installer.UninstallPluginAsync(
    installedPlugin,
    CancellationToken.None
)
```
4. List all installed plugins
```CSharp
var allInstalledPlugins = await ps.Installer.GetAllInstalledPluginsAsync(
    CancellationToken.None
)
```