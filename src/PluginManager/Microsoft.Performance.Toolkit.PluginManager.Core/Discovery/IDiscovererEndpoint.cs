using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    public interface IDiscovererEndpoint
    {
        /// <summary>
        ///     Discovers the latest version of all plugins available from a plugin source.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <returns>
        ///     A collection of plugins that are discovered from the given source.
        /// </returns>
        /// TODO: Add search
        Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverPluginsLatestAsync();


        /// <summary>
        ///     Discovers all versions of the given plugin from a plugin source.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <param name="pluginIdentity">
        ///     The target plugin to look up for.    
        /// </param>
        /// <returns>
        ///     All all versions of the plugin discovered from the given plugin source.
        /// </returns>
        Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverAllVersionsOfPlugin(PluginIdentity pluginIdentity);
    }
}
