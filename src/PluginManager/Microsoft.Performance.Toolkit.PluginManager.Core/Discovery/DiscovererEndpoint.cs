using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    public abstract class DiscovererEndpoint : IDiscovererEndpoint
    {
        protected DiscovererEndpoint() { }

        public abstract Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverAllVersionsOfPlugin(PluginIdentity pluginIdentity);

        public abstract Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverPluginsLatestAsync();
    }
}
