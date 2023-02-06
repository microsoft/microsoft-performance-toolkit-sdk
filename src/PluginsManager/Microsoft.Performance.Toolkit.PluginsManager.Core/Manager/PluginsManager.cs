// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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

        private readonly IResourceRepository<IPluginDiscovererProvider> discovererProviderRepository;
        private readonly IResourceRepository<IPluginFetcher> pluginFetcherRepository;

        private readonly DiscoverersManager discoverersManager;

        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviderRepo">
        ///     A repository of discoverer providers.
        /// </param>
        /// <param name="fetcherRepo">
        ///     A repository of plugin fetchers.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginManagerResource"/>s at run time.
        /// </param>
        public PluginsManager(
            IResourceRepository<IPluginDiscovererProvider> discovererProviderRepo,
            IResourceRepository<IPluginFetcher> fetcherRepo,
            IPluginManagerResourceLoader resourceLoader)
        {
            this.resourceLoader = resourceLoader;

            this.discovererProviderRepository = discovererProviderRepo;

            this.discoverersManager = new DiscoverersManager(
                this.discovererProviderRepository,
                new DiscoverersFactory());


            this.pluginFetcherRepository = fetcherRepo;

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
            var tasks = Task.WhenAll(this.PluginSources.Select(s => GetAvailablePluginsLatestFromSourceAsync(s, cancellationToken)));
            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from all plugin sources.
            // For each discovered plugin, the latest version across all sources will be returned.
            // For now, we assume:
            //      1. Duplicates (plugins with same id and version) maybe be discovered from different sources.
            //      2. In the case when duplicated "lastest" are discovered, only one of the duplicates will be returned.
            var results = new Dictionary<string, AvailablePlugin>();
            foreach (IReadOnlyCollection<AvailablePlugin> taskResult in tasks.Result)
            {
                IEnumerable<KeyValuePair<string, AvailablePlugin>> kvps = taskResult
                    .Select(p => new KeyValuePair<string, AvailablePlugin>(p.AvailablePluginInfo.Identity.Id, p));

                results.Union(kvps)
                       .GroupBy(g => g.Key)
                       .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value)
                                                       .OrderByDescending(x => x.AvailablePluginInfo.Identity.Version)
                                                       .ThenBy(x=>x.AvailablePluginInfo.PluginSource.Uri)
                                                       .ThenBy(x=>x.AvailablePluginInfo.PluginSource.Uri)
                                                       .First());
            }

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));

            if (!this.PluginSources.Contains(source))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            IPluginDiscoverer[] discoverers = this.discoverersManager.GetDiscoverersFromSource(source).ToArray();

            var tasks = Task.WhenAll(discoverers.Select(d => d.DiscoverPluginsLatestAsync(cancellationToken)));
            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from the same plugin source but by different discoverers.
            // If more than one available plugin with the same identity are discovered, exception should be thrown.
            var results = new Dictionary<string, AvailablePlugin>();
            for (int i = 0; i < tasks.Result.Length; i++)
            {
                IPluginDiscoverer discoverer = discoverers[i];
                
                foreach (KeyValuePair<string, AvailablePluginInfo> kvp in tasks.Result[i])
                {
                    string id = kvp.Key;
                    AvailablePluginInfo pluginInfo = kvp.Value;

                    if (!results.TryGetValue(id, out AvailablePlugin availablePlugin) ||
                        availablePlugin.AvailablePluginInfo.Identity.Version < pluginInfo.Identity.Version)
                    {

                        IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                        var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                        results[id] = newPlugin;
                    }
                    else if (availablePlugin.AvailablePluginInfo.Identity.Equals(pluginInfo.Identity))
                    {
                        throw new InvalidOperationException
                            ($"Duplicated plugins identity {pluginInfo.Identity.Id}-{pluginInfo.Identity.Version}, " +
                            $"found from source {source.Uri}");
                    }
                }
            }

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            var tasks = Task.WhenAll(this.PluginSources.Select(s => GetAllVersionsOfPluginFromSourceAsync(s, pluginIdentity, cancellationToken)));
            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines versions of plugin discovered from all plugin sources.
            // For now, we assume:
            //      1. Duplicates (same version) maybe discovered from different sources.
            //      2. In the case when duplicated versions are discovered, only one of them will be returned.
            var results = tasks.Result.SelectMany(x => x)
                                .GroupBy(x => x.AvailablePluginInfo.Identity)
                                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.AvailablePluginInfo.PluginSource.Uri)
                                                                .First());

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginFromSourceAsync(
            PluginSource source,
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            IPluginDiscoverer[] discoverers = this.discoverersManager.GetDiscoverersFromSource(source).ToArray();
            var tasks = Task.WhenAll(discoverers.Select(d => d.DiscoverAllVersionsOfPlugin(pluginIdentity, cancellationToken)));

            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from the same plugin source but by different discoverers.
            // If more than one available plugin with the same identity are discovered, exception should be thrown.
            var results = new Dictionary<PluginIdentity, AvailablePlugin>();     
            for (int i = 0; i < discoverers.Length; i++)
            {
                IPluginDiscoverer discoverer = discoverers[i];
                foreach (AvailablePluginInfo pluginInfo in tasks.Result[i])
                {
                    if (results.ContainsKey(pluginInfo.Identity))
                    {
                        throw new InvalidOperationException
                            ($"Duplicated plugins identity {pluginInfo.Identity.Id}-{pluginInfo.Identity.Version}, " +
                            $"found from source {source.Uri}");
                    }

                    IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                    var availablePlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[pluginInfo.Identity] = availablePlugin; 
                }
            }

            return results.Values;
        }

        private async Task<IPluginFetcher> GetPluginFetcher(AvailablePluginInfo availablePlugin)
        {
            foreach (IPluginFetcher fetcher in this.pluginFetcherRepository.PluginResources)
            {
                if (fetcher.TryGetGuid() == availablePlugin.FetcherResourceId && await fetcher.IsSupportedAsync(availablePlugin))
                {
                    return fetcher;
                }
            }

            throw new InvalidOperationException("No supported fetcher found");
        }
    }
}
