// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Extensibility;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Transport
{
    /// <summary>
    ///     Represents a fetcher that is capable of fetching the plugin package from a supported <see cref="AvailablePlugin"/>.
    ///     A fetcher is meant to be stateless.
    /// </summary>
    public interface IPluginFetcher : IPluginManagerResource
    {
        /// <summary>
        ///     Gets the identifier of this fetcher type.
        /// </summary>
        Guid TypeId { get; }

        /// <summary>
        ///     Checks if the given <paramref name="plugin"/> is supported by this fetcher.
        /// </summary>
        /// <param name="plugin">
        ///     The available plugin containing information for the fetcher to verify if it's supported.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="plugin"/> is supported by this fetcher. <c>false</c>
        ///     otherwise.
        /// </returns>
        Task<bool> IsSupportedAsync(AvailablePlugin plugin);

        /// <summary>
        ///     Asynchronously gets the plugin package associated with the given <see cref="AvailablePlugin"/> and returns as a stream.
        /// </summary>
        /// <param name="plugin">
        ///     The discovered plugin that is available for fetching.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin package downloading.    
        /// </param>
        /// <returns>
        ///     The stream of the plugin package file.
        /// </returns>
        Task<Stream> GetPluginStreamAsync(
            AvailablePlugin plugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}
