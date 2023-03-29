// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
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
        /// <param name="pluginSourceRepository">
        ///     The repository of plugin sources.
        /// </param>
        /// <param name="pluginDiscovererProviderRepository">
        ///     The repository of discoverer providers.
        /// </param>
        /// <param name="pluginFetcherRepository">
        ///     The repository of fetchers.
        /// </param>
        public PluginsSystem(
            IPluginsInstaller installer,
            IPluginsDiscoverer discoverer,
            IRepository<PluginSource> pluginSourceRepository,
            IPluginsSystemResourceLoader<IPluginDiscovererProvider> pluginDiscovererProviderLoader,
            IPluginsSystemResourceLoader<IPluginFetcher> pluginFetcherLoader)
        {
            Guard.NotNull(installer, nameof(installer));
            Guard.NotNull(discoverer, nameof(discoverer));
            Guard.NotNull(pluginSourceRepository, nameof(pluginSourceRepository));
            Guard.NotNull(pluginDiscovererProviderLoader, nameof(pluginDiscovererProviderLoader));
            Guard.NotNull(pluginFetcherLoader, nameof(pluginFetcherLoader));

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
        /// <param name="loggerFactory">
        ///     Used to create a logger for the given type.
        /// </param>
        /// <returns>
        ///     The created plugins system.
        /// </returns>
        public static PluginsSystem CreateFileBasedPluginsSystem(
            string root,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNullOrWhiteSpace(root, nameof(root));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            string pluginsSystemRoot = Path.GetFullPath(root);

            var pluginSourceRepo = new PluginSourceRepository();
            var fetcherRepo = new PluginsSystemResourceRepository<IPluginFetcher>();
            var discovererProviderRepo = new PluginsSystemResourceRepository<IPluginDiscovererProvider>();

            var discoverer = new PluginsDiscoverer(
                pluginSourceRepo,
                fetcherRepo,
                discovererProviderRepo,
                loggerFactory);

            var registry = new FileBackedPluginRegistry(
                pluginsSystemRoot,
                SerializationUtils.GetJsonSerializerWithDefaultOptions<List<InstalledPluginInfo>>(),
                loggerFactory);

            var packageReader = new ZipPluginPackageReader(
                SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>(),
                loggerFactory);

            var installer = new FileBackedPluginsInstaller(
                pluginsSystemRoot,
                registry,
                SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>(),
                new InstalledPluginDirectoryChecksumValidator(pluginsSystemRoot),
                packageReader,
                loggerFactory);

            return new PluginsSystem(
                installer,
                discoverer,
                pluginSourceRepo,
                discovererProviderRepo,
                fetcherRepo);
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
        public IPluginsSystemResourceLoader<IPluginFetcher> FetcherResourceLoader { get; }

        /// <summary>
        ///     Gets the loader of discoverer providers.
        /// </summary>
        public IPluginsSystemResourceLoader<IPluginDiscovererProvider> DiscovererProviderResourceLoader { get; }

        /// <summary>
        ///     Gets the repository of plugin sources.
        /// </summary>
        public IRepository<PluginSource> PluginSourceRepository { get; }
    }
}
