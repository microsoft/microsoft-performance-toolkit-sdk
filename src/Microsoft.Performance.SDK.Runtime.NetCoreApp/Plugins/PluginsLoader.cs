// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins
{
    /// <summary>
    ///     Handles loading SDK plugins into isolation assembly contexts and notifying
    ///     <see cref="IPluginsConsumer"/> listeners of all custom data sources loaded
    ///     by plugins.
    ///     <para/>
    ///     Plugins are loaded by supplying either <see cref="TryLoadPlugin(string)"/> or
    ///     <see cref="TryLoadPlugins(IEnumerable{string}, out IList{string})"/> directories
    ///     that contain all binaries necessary for the plugins to run. The filesystem structure
    ///     of plugins is expected to be 
    ///         InstalledPlugins\<manager_version>\<plugin_name>\<plugin_version>
    ///     For example, a plugin named "foo" with two versions, 1.0.0 and 2.0.0,
    ///     may have the following folder structure:
    ///         C:\SDK\InstalledPlugins\1.2.3\foo\1.0.0\<files>
    ///         C:\SDK\InstalledPlugins\1.2.3\foo\2.0.0\<files>
    ///     In this example, a specific version of "foo" can be loaded by passing in the full path of
    ///     the "1.0.0" or "2.0.0" folder to <see cref="TryLoadPlugin(string)"/> or <see cref="TryLoadPlugins(IEnumerable{string}, out IList{string})"/>.
    ///     Note that passing in the full path to "foo" or any of its parents will result in both
    ///     versions of the plugin "foo" being loaded.
    ///     <para/>
    ///     Failure to adhere to this filesystem structure will result in plugins being loaded with
    ///     an unknown (<c>null</c>) name and version.
    ///     <para/>
    ///     <see cref="IPluginsConsumer"/> listeners can register to be notified of plugins loaded in
    ///     the future through <see cref="Subscribe(IPluginsConsumer)"/>. When a new listener subscribes,
    ///     before <see cref="Subscribe(IPluginsConsumer)"/> returns the consumer is first notified
    ///     of all plugins loaded prior to the subscription.
    ///     <para/>
    ///     All calls to loading or obtaining loaded plugins are thread-safe occur in a linearizable order. 
    ///     Listeners are guaranteed to not miss any loaded custom data sources that get loaded concurrently
    ///     with their <see cref="Subscribe(IPluginsConsumer)"/> call.
    /// </summary>
    public class PluginsLoader
    {
        private readonly object mutex = new object();

        private readonly AssemblyExtensionDiscovery extensionDiscovery;

        private readonly IPlugInCatalog catalog;

        private readonly IDataExtensionRepositoryBuilder extensionRepository;

        private readonly ICollection<IPluginsConsumer> subscribers;

        public PluginsLoader()
        {
            this.subscribers = new ConcurrentSet<IPluginsConsumer>();
            this.extensionDiscovery = new AssemblyExtensionDiscovery(new IsolationAssemblyLoader());
            this.catalog = new ReflectionPlugInCatalog(this.extensionDiscovery);
            this.extensionRepository = new DataExtensionFactory().SingletonDataExtensionRepository;

            // The constructor for this object hooks up the repository to the extension provider
            // (the ExtensionDiscovery in this case). We do not need to hold onto this object
            // explicitly, only call its constructor.
            new DataExtensionReflector(this.extensionDiscovery, this.extensionRepository);
        }

        /// <summary>
        ///     All of the custom data sources that have been loaded by plugins.
        /// </summary>
        public IEnumerable<CustomDataSourceReference> LoadedCustomDataSources
        {
            get
            {
                IEnumerable<CustomDataSourceReference> copy;
                lock (this.mutex)
                {
                    copy = new HashSet<CustomDataSourceReference>(this.catalog.PlugIns);
                }
                return copy;
            }
        }

        /// <summary>
        ///     Attempts to load the plugin(s) contained within the given directory.
        /// </summary>
        /// <param name="directory">
        ///     The root directory containing the plugin(s) to attempt to load.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all plugins within the given directory were loaded, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        ///     If multiple plugins are contained within the given directory, this method will return
        ///     <c>false</c> if _any_ of them fail to load and not provide information as to which
        ///     plugin(s) failed. For example, if the filesystem looks like
        ///         rootFolder
        ///             pluginA
        ///                 ...files for pluginA...
        ///             pluginB
        ///                 ...files for pluginB...
        ///     TryLoadPlugin(rootFolder) will fail if either or both of pluginA or pluginB fail to load.
        ///     If the caller must know which plugin(s) failed, it must use <see cref="TryLoadPlugins(IEnumerable{string}, out IList{string})"/>.
        ///     <para/>
        ///     This method does not report an error if a different version of one of the custom data
        ///     sources being loaded has already been loaded by either the current or a previous call to <see cref="TryLoadPlugin(string)"/>
        ///     or <see cref="TryLoadPlugins(IEnumerable{string}, out IList{string})"/>.
        /// </remarks>
        public bool TryLoadPlugin(string directory)
        {
            return this.TryLoadPlugins(new List<string> { directory }, out _);
        }

        /// <summary>
        ///     Attempts to load the plugin(s) contained within any of the given directories.
        /// </summary>
        /// <param name="directories">
        ///     The directories to attempt to load plugins from.
        /// </param>
        /// <param name="failed">
        ///     The directories from <paramref name="directories"/> which contained at least
        ///     one plugin that failed to load. If this method returns <c>true</c>, this
        ///     will be empty.
        /// </param>
        /// <returns>
        ///     <c>true</c> if all plugins within the given directories were loaded, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        ///     A directory from <paramref name="directories"/> will be included in <paramref name="failed"/>
        ///     if _any_ of the plugins inside the directory fail to load. See remarks on <see cref="TryLoadPlugin(string)"/>
        ///     for more information.
        ///     <para/>
        ///     This method does not report an error if a different version of one of the custom data
        ///     sources being loaded has already been loaded by either the current or a previous call to <see cref="TryLoadPlugin(string)"/>
        ///     or <see cref="TryLoadPlugins(IEnumerable{string}, out IList{string})"/>.
        /// </remarks>
        public bool TryLoadPlugins(IEnumerable<string> directories, out IList<string> failed)
        {
            lock (this.mutex)
            {
                failed = new List<string>();
                var oldPlugins = new HashSet<CustomDataSourceReference>(this.catalog.PlugIns);
                foreach (var dir in directories)
                {

                    if (!this.extensionDiscovery.ProcessAssemblies(dir))
                    {
                        failed.Add(dir);
                        continue;
                    }
                }

                // TODO: this does redundant work re-finalizing custom data sources
                // loaded by previous calls to this method. The extension repository
                // should get refactored to avoid this redundant work.
                this.extensionRepository.FinalizeDataExtensions();

                foreach (var source in this.catalog.PlugIns.Except(oldPlugins))
                {
                    var info = this.GetPluginNameAndVersion(source);
                    this.NotifyCustomDataSourceLoaded(info.Item1, info.Item2, source);
                }

                return failed.Count == 0;
            }
        }

        /// <summary>
        ///     Registers an <see cref="IPluginsConsumer"/> to receive all future plugins, and sends all
        ///     previously loaded plugins to its <see cref="IPluginsConsumer.OnCustomDataSourceLoaded(string, CustomDataSourceReference)"/> handler.
        ///     <para/>
        ///     While the consumer is receiving all previously loaded plugins, this method blocks and
        ///     plugins cannot be loaded.
        /// </summary>
        /// <param name="consumer">The consumer to subscribe and receive all loaded plugins</param>
        public void Subscribe(IPluginsConsumer consumer)
        {
            lock (this.mutex)
            {
                // Manually send all the already loaded plugins to this consumer
                foreach (var source in this.catalog.PlugIns)
                {
                    var info = this.GetPluginNameAndVersion(source);
                    consumer.OnCustomDataSourceLoaded(info.Item1, info.Item2, source);
                }

                // Subscribe to all future plugin loads
                this.subscribers.Add(consumer);
            }
        }

        /// <summary>
        ///     Unregisters an <see cref="IPluginsConsumer"/> from plugins loaded in the future.
        /// </summary>
        /// <param name="consumer"></param>
        public void Unsubscribe(IPluginsConsumer consumer)
        {
            this.subscribers.Remove(consumer);
        }

        private Tuple<string, Version> GetPluginNameAndVersion(CustomDataSourceReference cds)
        {
            var fileName = cds.AssemblyPath;
            Guard.NotNullOrWhiteSpace(fileName, nameof(fileName));
            var parentFolder = new DirectoryInfo(fileName).Parent;

            string pluginName;
            Version pluginVersion;

            if (parentFolder?.Parent?.Parent?.Parent?.Name == PluginsConstants.InstalledPluginsRootFolderName)
            {
                // This plugin was installed by the plugin manager. It has its binaries inside of two nested folders.
                // The outermost folder is the plugin's name, and innermost folder is its version. So, we get
                // its name from the parent's parent.
                pluginName = parentFolder.Parent.Name;
                pluginVersion = Version.TryParse(parentFolder.Name, out var version) ? version : null;
            }
            else if (parentFolder?.Parent?.Name == CustomDataSourceConstants.CustomDataSourceRootFolderName)
            {
                // Legacy: this plugin was bundled with WPA. It has its binaries inside of one folder
                // named after the plugin itself.
                pluginName = parentFolder.Name;
                pluginVersion = null;
            }
            else
            {
                // Unknown file system schema. Do not attempt to make assumptions about the plugin's name.
                // This could happen when loading plugins through un-managed file system schemas (such as the
                // -addsearchdir option for WPA), or if the calling application does not store
                // plugin binaries in the correct way.
                pluginName = null;
                pluginVersion = null;
            }

            return new Tuple<string, Version>(pluginName, pluginVersion);
        }

        private void NotifyCustomDataSourceLoaded(string pluginName, Version pluginVersion, CustomDataSourceReference customDataSource)
        {
            Guard.NotNull(this.catalog, nameof(this.catalog));
            if (!this.catalog.IsLoaded)
            {
                throw new InvalidOperationException("All plugins must be loaded before this point");
            }

            foreach (var consumer in this.subscribers)
            {
                consumer.OnCustomDataSourceLoaded(pluginName, pluginVersion, customDataSource);
            }
        }
    }
}
