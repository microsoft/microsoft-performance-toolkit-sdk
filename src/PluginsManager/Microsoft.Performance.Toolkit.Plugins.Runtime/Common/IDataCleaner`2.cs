// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Cleans up data at a given location.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of data to clean up.
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The type of location to clean up.
    /// </typeparam>
    public interface IDataCleaner<TTarget, TEntity>
    {
        /// <summary>
        ///     Cleans up data at the given location.
        /// </summary>
        /// <param name="target">
        ///     The location to clean up.
        /// </param>
        /// <param name="fillter">
        ///     A predicate that determines whether or not a given item should be cleaned up.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of items that were cleaned up.
        /// </returns>
        IEnumerable<TEntity> CleanDataAt(
            TTarget target,
            Func<TEntity, bool> fillter,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Cleans up the given data.
        /// </summary>
        /// <param name="entity">
        ///     The data to clean up.
        /// </param>
        void CleanData(TEntity entity);
    }
}
