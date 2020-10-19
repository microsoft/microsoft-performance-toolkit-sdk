// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     Implemented so that data extensions may be registered. A data extension must be registered
    ///     before it can receive data for processing, or before its output may be exposed for consumption.
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
    public interface ISourceDataCookerRegistrar<T, TContext, TKey>
        : ISourceDataCookerRetrieval<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Gets access to all registered data cookers.
        /// </summary>
        IReadOnlyCollection<ISourceDataCooker<T, TContext, TKey>> RegisteredSourceDataCookers { get; }

        /// <summary>
        ///     Called to register an <see cref="IDataCooker"/> for use in processing a source.
        /// </summary>
        /// <param name="dataCooker">
        ///     The data extension to register for use.
        /// </param>
        void RegisterSourceDataCooker(ISourceDataCooker<T, TContext, TKey> dataCooker);
    }
}
