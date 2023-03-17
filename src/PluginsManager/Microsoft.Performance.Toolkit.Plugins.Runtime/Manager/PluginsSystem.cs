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
        ///     
        /// </param>
        public PluginsSystem(
            IPluginsInstaller installer,
            IPluginsDiscoveryManager discoverer)
        {
            this.Installer = installer;
            this.Discoverer = discoverer;
        }

        /// <summary>
        ///     Creates a file based plugins system.
        /// </summary>
        /// <param name="registryRoot">
        ///     The root directory of the registry.
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
            string registryRoot,
            IReadonlyRepository<PluginSource> pluginSourcesRepo,
            IPluginsManagerResourceRepository<IPluginFetcher> fetchersRepo,
            IPluginsManagerResourceRepository<IPluginDiscovererProvider> discovererProvidersRepo,
            ILogger logger)
        {
            IPluginRegistry registry = new FileBackedPluginRegistry(registryRoot);
            IPluginsInstaller installer = new FileBackedPluginsInstaller(registryRoot, registry);
            IPluginsDiscoveryManager discoverer = new PluginsDiscoveryManager(
                pluginSourcesRepo,
                fetchersRepo,
                discovererProvidersRepo,
                logger);

            return new PluginsSystem(installer, discoverer);
        }

        /// <summary>
        ///     Gets the installer.
        /// </summary>
        public IPluginsInstaller Installer { get; }

        /// <summary>
        ///     Gets the discoverer.
        /// </summary>
        public IPluginsDiscoveryManager Discoverer { get; }
    }
}
