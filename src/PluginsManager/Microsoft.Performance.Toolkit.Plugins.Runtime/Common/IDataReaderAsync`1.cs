// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data reader that can read data from a data source asynchronously.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The type of data source.
    /// </typeparam>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataReaderAsync<TSource, TEntity>
    {
        /// <summary>
        ///     Reads data from the given data source asynchronously.
        /// </summary>
        /// <param name="source">
        ///     The data source.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the operation should be canceled.
        /// </param>
        /// <returns>
        ///     The data entity read from the data source.
        /// </returns>
        Task<TEntity> ReadDataAsync(TSource source, CancellationToken cancellationToken);

        /// <summary>
        ///     Determines if the given data source can be read.
        /// </summary>
        /// <param name="source">
        ///     The data source.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the data source can be read; <c>false</c> otherwise.
        /// </returns>
        bool CanReadData(TSource source);
    }

}