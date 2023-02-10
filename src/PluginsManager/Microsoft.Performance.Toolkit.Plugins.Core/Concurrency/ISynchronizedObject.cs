// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Concurrency
{
    /// <summary>
    ///     Represents an object that facilitates grouping method calls into one synchronization region by surrounding them
    ///     with <see cref="ISynchronizedObject.AcquireLock(CancellationToken)"/> and <see cref="ISynchronizedObject.ReleaseLock"/>.
    /// </summary>
    public interface ISynchronizedObject
    {
        /// <summary>
        ///     Attempts to acquire the lock until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An await-able <see cref="Task"/> that, upon completion, indicates the lock has been acquired.
        /// </returns>
        Task AcquireLock(CancellationToken cancellationToken);

        /// <summary>
        ///     Releases the lock.
        /// </summary>
        void ReleaseLock();
    }
}
