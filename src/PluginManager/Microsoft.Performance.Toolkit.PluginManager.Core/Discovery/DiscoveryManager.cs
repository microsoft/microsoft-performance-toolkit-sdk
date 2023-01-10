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
    // TODO: Lazy initialize discoverers
    public class DiscoveryManager
    {
        private readonly ISet<IPluginSource> pluginSources;
        private readonly HashSet<IPluginDiscovererSource> discovererSources;
        private readonly IDiscovererSourceLoader loader;
        private readonly Dictionary<IPluginSource, IList<IPluginDiscoverer>> discoverers;

        public DiscoveryManager(
            IEnumerable<IPluginDiscovererSource> pluginDiscovererSources,
            IDiscovererSourceLoader discovererSourceLoader)
        {
            this.discovererSources = new HashSet<IPluginDiscovererSource>(pluginDiscovererSources);
            this.discoverers = new Dictionary<IPluginSource, IList<IPluginDiscoverer>>();
        }

        public IEnumerable<IPluginSource> PluginSources
        {
            get
            {
                return this.pluginSources;
            }
        }

        public IEnumerable<IPluginDiscovererSource> DiscovererSources
        {
            get
            {
                return this.discovererSources;
            }
        }

        public void ClearPluginSources()
        {
            this.pluginSources.Clear();
            this.discoverers.Clear();
        }

        public void AddPluginSource<TSource>(TSource source) where TSource : class, IPluginSource
        {
            Guard.NotNull(source, nameof(source));

            this.pluginSources.Add(source);
            this.discoverers.Add(source, new List<IPluginDiscoverer>());

            foreach (IPluginDiscovererSource<TSource> discovererSource in this.discovererSources.OfType<IPluginDiscovererSource<TSource>>())
            {
                if (discovererSource.IsSourceSupported(source))
                {
                    this.discoverers[source].Add(discovererSource.CreateDiscoverer(source));
                }
            }
        }

        public void LoadAdditionalDiscovererSource(string directory)
        {
            Guard.NotNull(directory, nameof(directory));

            if (!this.loader.TryLoad(directory, out IEnumerable<IPluginDiscovererSource> discovererSources))
            {
                return;
            }

            IEnumerable<IPluginDiscovererSource> newSources = discovererSources.Except(this.discovererSources);
                
            foreach (IPluginDiscovererSource source in newSources)
            {
                Type sourceType = source.PluginSourceType;
                Type discovererSourceType = source.GetType();

                foreach (KeyValuePair<IPluginSource, IList<IPluginDiscoverer>> kvp in this.discoverers)
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

            this.discovererSources.UnionWith(discovererSources);
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

            foreach (IPluginDiscoverer discoverer in this.discoverers[pluginSource])
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
                foreach (IPluginDiscoverer discoverer in this.discoverers[availablePlugin.PluginSource])
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
