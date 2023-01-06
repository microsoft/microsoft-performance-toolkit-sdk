// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Core APIs that discoverer <see cref="IAvailablePlugin"/>s from <see cref="IPluginSource"/>(s)
    /// </summary>
    public interface IPluginDiscoverer
    {
        /// <summary>
        ///     Discovers the latest version of all plugins available from a plugin source.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A collection of plugins that are discovered from the given source.
        /// </returns>
        /// TODO: Add search
        Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Discovers all versions of the given plugin from a plugin source.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <param name="pluginIdentity">
        ///     The target plugin to look up for.    
        /// </param>
        /// <param name="cancellationToken">
        /// </param>
        /// <returns>
        ///     All versions of the plugin discovered from the given plugin source.
        /// </returns>
        Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);
    }
}
