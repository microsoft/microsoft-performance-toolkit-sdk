// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a type that can copy a stream to a destination.
    /// </summary>
    /// <typeparam name="TDestination">
    ///     The type of the destination.
    /// </typeparam>
    public interface IStreamCopier<TDestination>
    {
        /// <summary>
        ///     Copies the given stream to the given destination.
        /// </summary>
        /// <param name="destination">
        ///     The destination to copy to.
        /// </param>
        /// <param name="stream">
        ///     The stream to copy.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A task that completes when the stream has been copied.
        /// </returns>
        Task CopyStreamAsync(
            TDestination destination,
            Stream stream,
            CancellationToken cancellationToken);
    }
}
