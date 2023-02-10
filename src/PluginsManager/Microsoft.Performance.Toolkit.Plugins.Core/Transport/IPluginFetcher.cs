// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Transport
{
    /// <summary>
    ///     Represents a fetcher that is capable of fetching the plugin package from a supported <see cref="AvailablePluginInfo"/>.
    ///     A fetcher is meant to be stateless.
    /// </summary>
    public interface IPluginFetcher : IPluginManagerResource
    {
        /// <summary>
        ///     Checks if the given <paramref name="pluginInfo"/> is supported by this fetcher.
        /// </summary>
        /// <param name="pluginInfo">
        ///     The available plugin info containing information for the fetcher to verify if it's supported.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="pluginInfo"/> is supported by this fetcher. <c>false</c>
        ///     otherwise.
        /// </returns>
        Task<bool> IsSupportedAsync(AvailablePluginInfo pluginInfo);

        /// <summary>
        ///     Asynchronously gets the plugin package associated with the given <see cref="AvailablePluginInfo"/> and returns as a stream.
        /// </summary>
        /// <param name="pluginInfo">
        ///     The information of a discovered plugin that is available for fetching.
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
            AvailablePluginInfo pluginInfo,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}
