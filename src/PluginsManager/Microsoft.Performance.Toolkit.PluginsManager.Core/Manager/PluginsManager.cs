// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Extensibility;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Transport;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager : IPluginsManager
    {
        private readonly IPluginManagerResourceLoader resourceLoader;

        private readonly ResourceRepository<IPluginDiscovererProvider> discovererProviderRepository;
        private readonly ResourceRepository<IPluginFetcher> pluginFetcherRepository;

        private readonly DiscoverersManager discoverersManager;

        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviders">
        ///     A collection of discoverer providers.
        /// </param>
        /// <param name="pluginFetchers">
        ///     A collection of plugin fetchers.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginManagerResource"/>s at run time.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> pluginFetchers,
            IPluginManagerResourceLoader resourceLoader)
        {
            this.resourceLoader = resourceLoader;
            
            this.discovererProviderRepository = 
                new ResourceRepository<IPluginDiscovererProvider>(discovererProviders);

            this.discoverersManager = new DiscoverersManager(
                this.discovererProviderRepository,
                new DiscoverersFactory());


            this.pluginFetcherRepository = new ResourceRepository<IPluginFetcher>(pluginFetchers);

            this.resourceLoader.Subscribe(this.discovererProviderRepository);
            this.resourceLoader.Subscribe(this.pluginFetcherRepository);
        }

        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        { 
            get
            {
                return this.discoverersManager.PluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.discoverersManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public void AddPluginSources(IEnumerable<PluginSource> sources)
        {
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

            if (!this.PluginSources.Contains(source))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            var results = new List<AvailablePlugin>();
            foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(source).AsQueryable())
            {
                IReadOnlyCollection<AvailablePlugin> plugins = await discoverer.DiscoverPluginsLatestAsync(cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPlugin(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            if (this.PluginSources.Contains(availablePlugin.PluginSource))
            {
                foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(availablePlugin.PluginSource).AsQueryable())
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

        /// <summary>
        ///     This is just an example of how a fetchers will be used. Will be moved to the installer.
        /// </summary>
        /// <param name="availablePlugin"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Stream> GetPluginPackageStreamAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            foreach (IPluginFetcher fetcher in this.pluginFetcherRepository.PluginResources)
            {
                if (fetcher.TypeId == availablePlugin.FetcherTypeId && await fetcher.IsSupportedAsync(availablePlugin))
                {
                    return await fetcher.GetPluginStreamAsync(availablePlugin, cancellationToken, progress);
                }
            }

            throw new InvalidOperationException("No supported fetcher found");
        }
    }
}
