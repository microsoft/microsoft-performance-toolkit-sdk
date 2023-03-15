// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data operator that can read and write data from a data source asynchronously.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The type of data source.
    /// </typeparam>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataReadWriteAsync<TSource, TEntity>
        : IDataReaderAsync<TSource, TEntity>,
          IDataWriterAsync<TSource, TEntity>
    {
    }
}
