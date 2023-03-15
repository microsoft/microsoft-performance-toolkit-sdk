// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data reader that can read data from a data source.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The type of data source.
    /// </typeparam>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataReader<TSource, TEntity>
    {
        /// <summary>
        ///     Reads data from the given data source.
        /// </summary>
        /// <param name="source">
        ///     The data source to read from.
        /// </param>
        /// <returns>
        ///     The data entity read from the data source.
        /// </returns>
        TEntity ReadData(TSource source);

        /// <summary>
        ///     Determines if the given data source can be read.
        /// </summary>
        /// <param name="source">
        ///     The data source to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the data source can be read; <c>false</c> otherwise.
        /// </returns>
        bool CanReadData(TSource source);
    }
}
