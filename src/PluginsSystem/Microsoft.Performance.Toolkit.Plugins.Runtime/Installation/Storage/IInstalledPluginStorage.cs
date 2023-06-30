// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a storage for installed plugins.
    /// </summary>
    public interface IInstalledPluginStorage
    {
        /// <summary>
        ///     Adds the given plugin package to the storage.
        /// </summary>
        /// <param name="package">
        ///     The plugin package to add to the storage.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token.
        /// </param>
        /// <param name="progress">
        ///     The progress of the operation.
        /// </param>
        /// <returns>
        ///     A task that completes when the plugin has been added to the storage.
        ///     The task returns the checksum of the added plugin.
        /// </returns>
        Task<string> AddAsync(
            PluginPackage package,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Removes the given plugin from the storage.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The plugin identity.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token.
        /// </param>
        /// <returns>
        ///     A task that completes when the plugin has been removed.
        /// </returns>
        Task RemoveAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Tries to get the plugin contents metadata for the given plugin identity.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The plugin identity.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token.
        /// </param>
        /// <returns>
        ///     A task that completes when the plugin contents metadata has been retrieved.
        /// </returns>
        Task<PluginContentsMetadata> TryGetPluginContentsMetadataAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);
    }
}
