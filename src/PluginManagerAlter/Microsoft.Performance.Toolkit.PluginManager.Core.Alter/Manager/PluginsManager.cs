// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Transport;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager : IPluginsManager
    {
        private readonly HashSet<PluginSource> pluginSources;
        private readonly IResourceLoader resourceLoader;

        private readonly ResourceRepository<IPluginDiscovererProvider> discovererProviderRepository;
        private readonly ResourceRepository<IPluginFetcher> pluginFetcherRepository;


        private readonly DiscoverersManager discoverersManager;

        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviders">
        ///     The known providers that have already been loaded or intialized directly.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginResource"/>s at run time.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> pluginFetchers,
            IResourceLoader resourceLoader)
        {
            this.resourceLoader = resourceLoader;
            
            this.discovererProviderRepository = 
                new ResourceRepository<IPluginDiscovererProvider>(discovererProviders);

            this.discoverersManager = new DiscoverersManager(
                this.discovererProviderRepository,
                new DiscoverersFactory());

            this.pluginSources = new HashSet<PluginSource>();


            this.pluginFetcherRepository = new ResourceRepository<IPluginFetcher>(pluginFetchers);

            this.resourceLoader.Subscribe(this.discovererProviderRepository);
            this.resourceLoader.Subscribe(this.pluginFetcherRepository);
        }

        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        { 
            get
            {
                return this.pluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.pluginSources.Clear();
            this.discoverersManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public void AddPluginSources(IEnumerable<PluginSource> sources)
        {
            this.pluginSources.UnionWith(sources);
            this.discoverersManager.AddPluginSources(sources);
        }

        /// <inheritdoc />
        public void LoadAdditionalPluginResources(string directory)
        {
            this.resourceLoader.TryLoad(directory);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            var results = new List<AvailablePlugin>();

            foreach (PluginSource pluginSource in this.PluginSources)
            {
                IReadOnlyCollection<AvailablePlugin> plugins = await GetAvailablePluginsLatestFromSourceAsync(pluginSource, cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));

            if (!this.pluginSources.Contains(source))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(source))
            {
                IReadOnlyCollection<AvailablePlugin> plugins = await discoverer.DiscoverPluginsLatestAsync(cancellationToken);

                // Stop calling other discoverers if plugins are found from one discoverer
                if (plugins.Any())
                {
                    return plugins;
                }
            }

            return Array.Empty<AvailablePlugin>();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPlugin(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            if (this.pluginSources.Contains(availablePlugin.PluginSource))
            {
                foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(availablePlugin.PluginSource))
                {
                    IReadOnlyCollection<AvailablePlugin> plugins = await discoverer.DiscoverAllVersionsOfPlugin(
                        availablePlugin.Identity,
                        cancellationToken);

                    if (plugins.Any())
                    {
                        return plugins;
                    }
                }
            }

            return Array.Empty<AvailablePlugin>();
        }


        public async Task<Stream> GetPluginPackageStreamAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            foreach (IPluginFetcher fetcher in this.pluginFetcherRepository.PluginResources)
            {
                if (fetcher.HostTypeGuid == availablePlugin.FetcherTypeId && fetcher.CanFetch(availablePlugin))
                {
                    await fetcher.GetPluginStreamAsync(availablePlugin, cancellationToken, progress);
                }
            }

            return null;
        }
    }
}
