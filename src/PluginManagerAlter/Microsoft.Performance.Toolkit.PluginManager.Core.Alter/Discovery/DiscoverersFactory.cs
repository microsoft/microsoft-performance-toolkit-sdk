using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    public sealed class DiscoverersFactory
    {
        public IEnumerable<IPluginDiscoverer> CreateDiscoverers(
            PluginSource pluginSourceEndpoint,
            IEnumerable<IPluginDiscovererProvider> providers)
        {
            IList<IPluginDiscoverer> results = new List<IPluginDiscoverer>();
            foreach (IPluginDiscovererProvider provider in providers)
            {
                if (provider.CanDiscover(pluginSourceEndpoint))
                {
                    results.Add(provider.CreateDiscoverer(pluginSourceEndpoint));
                }
            }

            return results;
        }
    }
}
