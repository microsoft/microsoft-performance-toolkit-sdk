// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery
{
    /// <summary>
    ///    A discoverer that is capable of discovering <see cref="AvailablePlugin"/>.
    /// </summary>
    public interface IPluginDiscoverer
    {
        /// <summary>
        ///     Gets the Guid that identifies the <see cref="IPluginDiscovererProvider"/> this discoverer is created from.
        /// </summary>
        Guid DiscovererResourceId { get; }

        /// <summary>
        ///     Discovers the latest version of all plugins.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
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
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     All versions of the plugin discovered.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> DiscoverAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Returns the metadata of a given plugin. 
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the requested plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The metadata of the given plugin.
        /// </returns>
        Task<PluginMetadata> GetPluginMetadataAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);
    }
}
