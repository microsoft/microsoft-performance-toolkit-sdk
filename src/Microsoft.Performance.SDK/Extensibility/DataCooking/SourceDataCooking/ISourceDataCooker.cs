// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     Flags to modify data cooker behavior.
    /// </summary>
    [Flags]
    public enum SourceDataCookerOptions
    {
        /// <summary>
        ///     Default behavior.
        /// </summary>
        None,

        /// <summary>
        ///     Rather than specifying specific data keys to filter data this cooker receives, it will receive all data
        ///     elements from the source parser.
        /// </summary>
        /// <remarks>
        ///     This will have a performance impact, and should only be used when necessary.
        /// </remarks>
        ReceiveAllDataElements,
    }

    /// <summary>
    ///     Implement this interface to receive data from a given source as it is being processed.
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
    public interface ISourceDataCooker<T, TContext, TKey>
        : IDataCooker,
          ISourceDataCookerDependencyTypes
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Gets a set of identifiers that determine which data from the source will
        ///     be forwarded to this extension.
        /// </summary>
        ReadOnlyHashSet<TKey> DataKeys { get; }

        /// <summary>
        ///     Gets any additional options that apply to this cooker.
        /// </summary>
        SourceDataCookerOptions Options { get; }

        /// <summary>
        ///     This is called just before source parsing begins for the pass through the
        ///     source for which this data cooker will receive data elements to process.
        /// </summary>
        /// <param name="dependencyRetrieval">
        ///     Used to retrieve data from other source data cookers.
        /// </param>
        /// <param name="cancellationToken">
        ///     Used to indicate that the user wishes to cancel.
        /// </param>
        void BeginDataCooking(ICookedDataRetrieval dependencyRetrieval, CancellationToken cancellationToken);

        /// <summary>
        ///     Called to cook raw data from the source.
        /// </summary>
        /// <param name="data">
        ///     The data from the source to be processed.
        /// </param>
        /// <param name="context">
        ///     Additional information to assist processing.
        /// </param>
        /// <param name="cancellationToken">
        ///     Used to indicate that the user wishes to cancel.
        /// </param>
        /// <returns>
        ///     The result from processing the data.
        /// </returns>
        DataProcessingResult CookDataElement(T data, TContext context, CancellationToken cancellationToken);

        /// <summary>
        ///     This is called at the end of the pass through the source for which this
        ///     data cooker will received data elements to process.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Used to indicate that the user wishes to cancel.
        /// </param>
        void EndDataCooking(CancellationToken cancellationToken);
    }
}
