## Glossary & Core Concepts
**Plugin** - Conceptually, a plugin is dynamically loaded software that can process traces. A plugin may contain one or more processing sources.  In our current system,  unlike processing source,  plugin doesn't have a tangible form.  As we introduce *plugin package* to our ecosystem, plugin becomes tangible. See **Plugin Package** for explanation.

**Processing Source** - An entry point for a plugin. A processing source declares a human-readable name and description, defines the data sources it supports, creates the data processor that will process instances of supported data sources and advertises the tables that may be built in response to data sources being processed

**Plugin Package** - A zip file with a custom extension (e.g. *.plugin*) containing the content of a plugin along with a metadata file that describes the plugin(s).  A plugin package may contain only one plugin. In other words, whatever is packaged into a plugin package by the the plugin author will be called **a** plugin in our new ecosystem. For v1, a plugin package will be self-contained in the way that it doesn't have dependencies on any other non-plugin packages. For example, a plugin must include Newtonsoft.Json if it requires it for JSON parsing.

**Plugin Metadata** - Information of a plugin. Contains the plugin's name, version, description, owners, SDK version and information about the processing sources and data cookers it provides.

**Plugin Consumer** - A client software that consumes a plugin package to get access to plugins and their processing sources. E.g., wpa.exe, the Engine.
- **Plugin Loader** - Used by the plugin consumer to load a set of plugins supplied by the plugin packages.

**Plugins System** - Used by the plugin consumer to manage the discovering, installing, uninstalling and updating of plugins.

#### Different Forms of Plugin
 - **Available Plugin**  - A plugin package discovered from a plugin source that is available for a plugin consumer to install.
 - **Installed Plugin** - A plugin that a user has opted to install. Installed plugins reside on the user's machine and can be loaded by a plugin consumer.
  - **Loaded Plugin** - A plugin that has been previously installed from a plugin package and has been loaded by the plugin loader for trace processing within an instance of a plugin consumer being opened.

**Plugin Source** - A source (e.g. a NuGet feed, a REST API, a text file) where plugins can be discovered from. The source provides information about how a plugin package can be transported to a plugin consumer.

**Plugin Installation** - A plugin installation involves two parts:
1. Taking a plugin package and processing its contents - extracting contents to some folder.
2. Recording installation to a *registry* - noting which version of which plugin was installed to which location.
Installation does not require a feed. A plugin can be installed from a local plugin package.

**Plugin Registry** - A local registry has two components: a registry file that records information of installed plugins and an ephemeral lock file that indicates whether a process is currently interacting with the registry. Local registry is on a per-consumer basis. (e.g. WPA may have its own registry that is different from some other consumer using the engine)
- **Plugin Package Cache** - Simply a directory containing plugin packages that may currently be installed.
Only one version of a plugin may be registered at a time. Multiple versions of a plugin can be stored in the cache.