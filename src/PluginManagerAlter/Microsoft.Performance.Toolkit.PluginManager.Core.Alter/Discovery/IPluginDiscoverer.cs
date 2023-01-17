// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    /// <summary>
    ///    A discoverer that discovers <see cref="AvailablePlugin"/>
    /// </summary>
    public interface IPluginDiscoverer
    {
        /// <summary>
        ///     Discovers the latest version of all plugins.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A collection of plugins that are discovered.
        /// </returns>
        /// TODO: Add search
        Task<IReadOnlyCollection<AvailablePlugin>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Discovers all versions of the given plugin.
        /// <param name="pluginIdentity">
        ///     The target plugin to look up for.    
        /// </param>
        /// <param name="cancellationToken">
        /// </param>
        /// <returns>
        ///     All versions of the plugin discovered.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> DiscoverAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);
    }
}
