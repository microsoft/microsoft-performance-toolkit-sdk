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
            IRepositoryRO<IPluginDiscovererProvider> pluginDiscovererProviderRepository,
            IRepositoryRO<IPluginFetcher> pluginFetcherRepository)
        {
            this.Installer = installer;
            this.Discoverer = discoverer;
            this.PluginSourcesRepository = pluginSourceRepository;
            this.DiscovererProvidersRepository = pluginDiscovererProviderRepository;
            this.FetchersRepository = pluginFetcherRepository;
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
            IRepository<PluginSource> pluginSourcesRepo = new PluginSourceRepository();
            IRepositoryRO<IPluginFetcher> fetchersRepo = new PluginsManagerResourceRepository<IPluginFetcher>();
            IRepositoryRO<IPluginDiscovererProvider> discovererProvidersRepo = new PluginsManagerResourceRepository<IPluginDiscovererProvider>();

            IPluginsDiscoverer discoverer = new PluginsDiscoverer(
              pluginSourcesRepo,
              fetchersRepo,
              discovererProvidersRepo,
              logger);


            IPluginRegistry registry = new FileBackedPluginRegistry(root);
            IPluginsInstaller installer = new FileBackedPluginsInstaller(root, registry);
          
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
        ///     Gets the repository of plugin fetchers.
        /// </summary>
        public IRepositoryRO<IPluginFetcher> FetchersRepository { get; }

        /// <summary>
        ///     Gets the repository of discoverer providers.
        /// </summary>
        public IRepositoryRO<IPluginDiscovererProvider> DiscovererProvidersRepository { get; }

        /// <summary>
        ///     Gets the repository of plugin sources.
        /// </summary>
        public IRepository<PluginSource> PluginSourcesRepository { get; }
    }
}
