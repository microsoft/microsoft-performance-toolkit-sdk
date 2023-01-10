// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Manages plugin discovery related resources.
    /// </summary>
    // TODO: Lazy initialize discoverers
    public class DiscoveryManager
    {
        private readonly ISet<IPluginSource> pluginSources;
        private readonly HashSet<IPluginDiscovererProvider> discovererProviders;
        private readonly IDiscovererProvidersLoader loader;
        private readonly Dictionary<IPluginSource, IList<IPluginDiscoverer>> sourceToDiscoverers;

        public DiscoveryManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IDiscovererProvidersLoader discovererSourceLoader)
        {
            this.pluginSources = new HashSet<IPluginSource>();
            this.loader = discovererSourceLoader;
            this.discovererProviders = new HashSet<IPluginDiscovererProvider>(discovererProviders);
            this.sourceToDiscoverers = new Dictionary<IPluginSource, IList<IPluginDiscoverer>>();
        }

        public IEnumerable<IPluginSource> PluginSources
        {
            get
            {
                return this.pluginSources;
            }
        }

        public IEnumerable<IPluginDiscovererProvider> DiscovererProviders
        {
            get
            {
                return this.discovererProviders;
            }
        }

        public void ClearPluginSources()
        {
            this.pluginSources.Clear();
            this.sourceToDiscoverers.Clear();
        }

        public void AddPluginSource<TSource>(TSource source) where TSource : class, IPluginSource
        {
            Guard.NotNull(source, nameof(source));

            this.pluginSources.Add(source);
            this.sourceToDiscoverers.Add(source, new List<IPluginDiscoverer>());

            foreach (IPluginDiscovererProvider<TSource> provider in this.discovererProviders.OfType<IPluginDiscovererProvider<TSource>>())
            {
                if (provider.IsSourceSupported(source))
                {
                    this.sourceToDiscoverers[source].Add(provider.CreateDiscoverer(source));
                }
            }
        }

        public void LoadAdditionalDiscovererSource(string directory)
        {
            Guard.NotNull(directory, nameof(directory));

            if (!this.loader.TryLoad(directory, out IEnumerable<IPluginDiscovererProvider> loadedProviders))
            {
                return;
            }

            IEnumerable<IPluginDiscovererProvider> newProviders = loadedProviders.Except(this.discovererProviders);
            this.discovererProviders.UnionWith(newProviders);

            foreach (IPluginDiscovererProvider source in newProviders)
            {
                Type sourceType = source.PluginSourceType;
                Type discovererSourceType = source.GetType();

                foreach (KeyValuePair<IPluginSource, IList<IPluginDiscoverer>> kvp in this.sourceToDiscoverers)
                {
                    IPluginSource pluginSource = kvp.Key;
                    if (sourceType !=  pluginSource.GetType())
                    {
                        continue;
                    }

                    if ((bool)discovererSourceType.GetMethod("IsSourceSupported")
                            .MakeGenericMethod(sourceType)
                            .Invoke(source, new object[] { pluginSource }))
                    {
                        var discoverer = (IPluginDiscoverer)discovererSourceType.GetMethod("CreateDiscoverer")
                            .MakeGenericMethod(sourceType)
                            .Invoke(source, new object[] { pluginSource });

                        kvp.Value.Add(discoverer);
                    }
                }
            }
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            IPluginSource pluginSource,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginSource, nameof(pluginSource));

            if (!this.pluginSources.Contains(pluginSource))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            foreach (IPluginDiscoverer discoverer in this.sourceToDiscoverers[pluginSource])
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await discoverer.DiscoverPluginsLatestAsync(cancellationToken);
                
                // Stop calling other discoverers if plugins are found from one discoverer
                if (plugins.Any())
                {
                    return plugins;
                }
            }

            return Array.Empty<IAvailablePlugin>();
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAllAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            //TODO: Concurrent
            var results = new List<IAvailablePlugin>();

            foreach (IPluginSource pluginSource in this.pluginSources)
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await GetAvailablePluginsLatestFromSourceAsync(pluginSource, cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(
            IAvailablePlugin availablePlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            if (this.pluginSources.Contains(availablePlugin.PluginSource))
            {
                foreach (IPluginDiscoverer discoverer in this.sourceToDiscoverers[availablePlugin.PluginSource])
                {
                    IReadOnlyCollection<IAvailablePlugin> plugins = await discoverer.DiscoverAllVersionsOfPlugin(
                        availablePlugin.Identity,
                        cancellationToken);

                    if (plugins.Any())
                    {
                        return plugins;
                    }
                }
            }

            return Array.Empty<IAvailablePlugin>();
        }
    }
}
