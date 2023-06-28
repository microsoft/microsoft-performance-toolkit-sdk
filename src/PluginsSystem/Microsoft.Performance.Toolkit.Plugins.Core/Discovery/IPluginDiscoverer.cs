// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Discovery
{
    /// <summary>
    ///    A discoverer that is capable of discovering <see cref="AvailablePluginInfo"/>.
    /// </summary>
    public interface IPluginDiscoverer
    {
        /// <summary>
        ///     Discovers the latest version of all plugins.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The <see cref="AvailablePluginInfo"/> of all discovered plugins.
        /// </returns>
        /// TODO: 243 Add search
        Task<IReadOnlyCollection<AvailablePluginInfo>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Discovers all versions of the given plugin.
        /// <param name="pluginIdentity">
        ///     The target plugin to look up for.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The <see cref="AvailablePluginInfo"/> of all versions of the given plugin.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePluginInfo>> DiscoverAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Returns the content of a given plugin.
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
        Task<PluginContents> GetPluginContentsAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Provides the <see cref="IPluginDiscoverer"/> an application-appropriate logging mechanism.
        /// </summary>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        void SetLogger(ILogger logger);
    }
}
