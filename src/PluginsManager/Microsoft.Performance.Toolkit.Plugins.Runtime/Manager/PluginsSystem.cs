// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <summary>
    ///    Represents a plugins system that can be used to discover and install plugins.
    /// </summary>
    public sealed class PluginsSystem
    {
        /// <summary>
        ///     Creates an instance of the <see cref="PluginsSystem"/>.
        /// </summary>
        /// <param name="installer">
        ///     The installer to use.
        /// </param>
        /// <param name="discoverer">
        ///     The discoverer to use.
        /// </param>
        private PluginsSystem(
            IPluginsInstaller installer,
            IPluginsDiscoverer discoverer,
            IRepository<PluginSource> pluginSourceRepository,
            IPluginsManagerResourceLoader<IPluginDiscovererProvider> pluginDiscovererProviderLoader,
            IPluginsManagerResourceLoader<IPluginFetcher> pluginFetcherLoader)
        {
            this.Installer = installer;
            this.Discoverer = discoverer;
            this.PluginSourceRepository = pluginSourceRepository;
            this.DiscovererProviderResourceLoader = pluginDiscovererProviderLoader;
            this.FetcherResourceLoader = pluginFetcherLoader;
        }

        /// <summary>
        ///     Creates a file based plugins system.
        /// </summary>
        /// <param name="root">
        ///     The root directory of the registry and installed plugins.
        /// </param>
        /// <param name="pluginSourcesRepo">
        ///     The repository of plugin sources.
        /// </param>
        /// <param name="fetchersRepo">
        ///     The repository of fetchers.
        /// </param>
        /// <param name="discovererProvidersRepo">
        ///     The repository of discoverer providers.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        /// <returns>
        ///     The created plugins system.
        /// </returns>
        public static PluginsSystem CreateFileBasedPluginsSystem(
            string root,
            ILogger logger)
        {
            var pluginSourcesRepo = new PluginSourceRepository();
            var fetchersRepo = new PluginsManagerResourceRepository<IPluginFetcher>();
            var discovererProvidersRepo = new PluginsManagerResourceRepository<IPluginDiscovererProvider>();

            var discoverer = new PluginsDiscoverer(
              pluginSourcesRepo,
              fetchersRepo,
              discovererProvidersRepo,
              logger);


            var registry = new FileBackedPluginRegistry(root);
            var installer = new FileBackedPluginsInstaller(root, registry);
          
            return new PluginsSystem(
                installer,
                discoverer,
                pluginSourcesRepo,
                discovererProvidersRepo,
                fetchersRepo);
        }

        /// <summary>
        ///     Gets the installer.
        /// </summary>
        public IPluginsInstaller Installer { get; }

        /// <summary>
        ///     Gets the discoverer.
        /// </summary>
        public IPluginsDiscoverer Discoverer { get; }

        /// <summary>
        ///     Gets the loader of fetchers.
        /// </summary>
        public IPluginsManagerResourceLoader<IPluginFetcher> FetcherResourceLoader { get; }

        /// <summary>
        ///     Gets the loader of discoverer providers.
        /// </summary>
        public IPluginsManagerResourceLoader<IPluginDiscovererProvider> DiscovererProviderResourceLoader { get; }

        /// <summary>
        ///     Gets the repository of plugin sources.
        /// </summary>
        public IRepository<PluginSource> PluginSourceRepository { get; }
    }
}
