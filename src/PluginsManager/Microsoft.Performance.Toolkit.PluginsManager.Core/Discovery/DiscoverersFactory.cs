using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery
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
        public async Task<IEnumerable<IPluginDiscoverer>> CreateDiscoverers(
            PluginSource pluginSource,
            IEnumerable<IPluginDiscovererProvider> providers)
        {
            IList<IPluginDiscoverer> results = new List<IPluginDiscoverer>();
            foreach (IPluginDiscovererProvider provider in providers)
            {
                if (await provider.IsSupportedAsync(pluginSource))
                {
                    results.Add(provider.CreateDiscoverer(pluginSource));
                }
            }

            return results;
        }
    }
}
