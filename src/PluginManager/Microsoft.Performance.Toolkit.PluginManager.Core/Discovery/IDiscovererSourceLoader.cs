using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    public interface IDiscovererSourceLoader
    {
         bool TryLoad(string directory, out IEnumerable<IPluginDiscovererSource> discovererSources);
    }
}
