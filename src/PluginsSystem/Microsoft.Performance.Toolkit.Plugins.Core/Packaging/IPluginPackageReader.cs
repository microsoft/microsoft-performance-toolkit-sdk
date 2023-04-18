// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging
{
    /// <summary>
    ///     Represents a reader that can read a <see cref="PluginPackage"/> from a stream.
    /// </summary>
    public interface IPluginPackageReader
    {
        /// <summary>
        ///     Tries to read a plugin package from the given stream.
        /// </summary>
        /// <param name="stream">
        ///     The stream to read the plugin package from.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the operation should be canceled.
        /// </param>
        /// <returns>
        ///     The plugin package, if it could be read from the stream. Otherwise, <c>null</c>.
        /// </returns>
        Task<PluginPackage> TryReadPackageAsync(
            Stream stream,
            CancellationToken cancellationToken);
    }
}