// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>	
    ///     Represents an object that facilitates grouping method calls into one synchronization region.	
    /// </summary>
    public interface ISynchronizedObject
    {
        /// <summary>
        ///     Acquires the lock asynchronously, failing with <see cref="TimeoutException"/> if the attempt times out.
        ///     Use in a using statement to make sure the lock is released.
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="timeout">
        ///     The timeout for the lock acquisition. If <c>null</c>, the lock will be acquired without a timeout.
        /// </param>
        /// <returns>
        ///     A task that completes when the lock is acquired.
        ///     The task result is an <see cref="IDisposable"/> that releases the lock when disposed.
        /// </returns>    
        Task<IDisposable> AquireLockAsync(CancellationToken cancellationToken, TimeSpan? timeout);
    }
}
