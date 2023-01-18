using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    public sealed class DiscoverersFactory
    {
        /// <summary>
        ///     Creates discoverer instances for a plugin source given a collection of providers.
        /// </summary>
        /// <param name="pluginSource">
        ///     A plugin source.
        /// </param>
        /// <param name="providers">
        ///     A collection of discoverer providers.
        /// </param>
        /// <returns></returns>
        public IEnumerable<IPluginDiscoverer> CreateDiscoverers(
            PluginSource pluginSource,
            IEnumerable<IPluginDiscovererProvider> providers)
        {
            IList<IPluginDiscoverer> results = new List<IPluginDiscoverer>();
            foreach (IPluginDiscovererProvider provider in providers)
            {
                if (provider.IsSupported(pluginSource))
                {
                    results.Add(provider.CreateDiscoverer(pluginSource));
                }
            }

            return results;
        }
    }
}
