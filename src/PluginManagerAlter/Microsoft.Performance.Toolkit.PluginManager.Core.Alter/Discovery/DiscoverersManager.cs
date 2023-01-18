// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Extensibility;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    /// <summary>
    ///     Manages a mapping from plugins sources to plugin discoverers.
    /// </summary>
    public sealed class DiscoverersManager
    {
        private readonly ResourceRepository<IPluginDiscovererProvider> repository;
        private readonly DiscoverersFactory discoverersFactory;
        private readonly ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;

        public DiscoverersManager(
            ResourceRepository<IPluginDiscovererProvider> discovererRepository,
            DiscoverersFactory discoverersFactory) 
        {
            this.repository = discovererRepository;
            this.repository.ResourcesAdded += OnNewProvidersAdded;
            this.discoverersFactory = discoverersFactory;
            this.sourceToDiscoverers = new ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>>();
        }

        public IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.sourceToDiscoverers.Keys;
            }
        }

        public IEnumerable<IPluginDiscoverer> GetDiscoverersFromSource(PluginSource source)
        {
            Guard.NotNull(source, nameof(source));

            if (this.sourceToDiscoverers.TryGetValue(source, out List<IPluginDiscoverer> disoverers))
            {
                return disoverers;
            }

            return Array.Empty<IPluginDiscoverer>();
        }

        public void ClearPluginSources()
        {
            this.sourceToDiscoverers.Clear();
        }

        public void AddPluginSources(IEnumerable<PluginSource> sources)
        {
            Guard.NotNull(sources, nameof(sources));

            foreach (PluginSource source in sources)
            {
                if (this.sourceToDiscoverers.ContainsKey(source))
                {
                    continue;
                }

                var discoverers = this.discoverersFactory.CreateDiscoverers(source, this.repository.PluginResources).ToList();
                this.sourceToDiscoverers.TryAdd(source, discoverers);
            }
        }

        private void OnNewProvidersAdded(object sender, NewResourcesEventArgs<IPluginDiscovererProvider> e)
        {
            foreach (KeyValuePair<PluginSource, List<IPluginDiscoverer>> kvp in this.sourceToDiscoverers)
            {
                kvp.Value.AddRange(this.discoverersFactory.CreateDiscoverers(kvp.Key, e.NewPluginResources));
            }
        }

    }
}
