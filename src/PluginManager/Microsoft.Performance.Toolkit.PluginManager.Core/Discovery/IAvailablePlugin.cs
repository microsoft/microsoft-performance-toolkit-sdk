// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Represents a discovered plugin container that Exposes plugin info and APIs for getting plugin metadata, plugin pakcage,
    ///     and other available versions of this plugin.
    /// </summary>
    public interface IAvailablePlugin
    {
        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the basic information of this plugin
        /// </summary>
        PluginInfo Info { get; }

        /// <summary>
        ///     Asynchronously gets the <see cref="Packaging.Metadata.PluginMetadata"/> of this plugin.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PluginMetadata> GetPluginMetadataAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken,
            bool force = false);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        Task<PluginPackage> GetPluginPackageAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken,
            bool force = false);

        /// <summary>
        ///     Asynchronously gets the <see cref="Packaging.PluginPackage"/> file and returns it as a stream.
        /// </summary>
        /// <param name="progress">
        ///     Indicates the progress of plugin package downloading.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The stream of the plugin package file.
        /// </returns>
        Task<Stream> GetPluginPackageStreamAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
