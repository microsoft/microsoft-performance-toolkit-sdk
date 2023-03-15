// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data writer that can write data to a target asynchronously.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The type of target location to write to.
    /// </typeparam>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataWriterAsync<TTarget, TEntity>
    {
        /// <summary>
        ///     Writes data to the given target location.
        /// </summary>
        /// <param name="target">
        ///     The target location to write to.
        /// </param>
        /// <param name="entity">
        ///     The data entity to write.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the operation should be canceled.
        /// </param>
        /// <returns>
        ///     A task that completes when the write operation is complete.
        /// </returns>
        Task WriteDataAsync(TTarget target, TEntity entity, CancellationToken cancellationToken);
    }
}
