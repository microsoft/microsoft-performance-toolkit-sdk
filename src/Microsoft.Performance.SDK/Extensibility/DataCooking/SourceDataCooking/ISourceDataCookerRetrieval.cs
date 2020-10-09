// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     Implemented to access source data cookers.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="Type"/> of data from the source to be processed.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     <see cref="Type"/> that contains context about the data from the source.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     <see cref="Type"/> that will be used to identify data from the source that is relevant to this extension.
    /// </typeparam>
    public interface ISourceDataCookerRetrieval<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Call to retrieve an instance of the cooker id.
        /// </summary>
        /// <param name="cookerPath">
        ///     Identify a cooker.
        /// </param>
        /// <returns>
        ///     Data cooker.
        /// </returns>
        ISourceDataCooker<T, TContext, TKey> GetSourceDataCooker(DataCookerPath cookerPath);
    }
}
