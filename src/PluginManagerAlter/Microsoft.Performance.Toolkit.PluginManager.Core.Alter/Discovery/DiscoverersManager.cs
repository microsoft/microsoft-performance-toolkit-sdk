using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    public sealed class DiscoverersManager
    {
        private readonly ResourceRepository<IPluginDiscovererProvider> repository;
        private readonly DiscoverersFactory discoverersFactory;
        private readonly Dictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;

        public DiscoverersManager(
            ResourceRepository<IPluginDiscovererProvider> discovererRepository,
            DiscoverersFactory discoverersFactory) 
        {
            this.repository = discovererRepository;
            this.repository.ResourcesChanged += OnNewProvidersAdded;
            this.discoverersFactory = discoverersFactory;
            this.sourceToDiscoverers = new Dictionary<PluginSource, List<IPluginDiscoverer>>();
        }

        private void OnNewProvidersAdded(object sender, NewResourcesEventArgs<IPluginDiscovererProvider> e)
        {
            foreach (KeyValuePair<PluginSource, List<IPluginDiscoverer>> kvp in this.sourceToDiscoverers)
            {
                kvp.Value.AddRange(this.discoverersFactory.CreateDiscoverers(kvp.Key, e.NewPluginResources));
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
                this.sourceToDiscoverers.Add(source, discoverers);
            }
        }
    }
}
