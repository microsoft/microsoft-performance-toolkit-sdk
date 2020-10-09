// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     Adds a method used to process a data source.
    ///         <remarks>
    ///         This is expected to be called by
    ///         <see cref="ICustomDataProcessorWithSourceParser"/> during <see cref="ICustomDataProcessor.ProcessAsync"/>.
    ///         </remarks>
    /// </summary>
    /// <typeparam name="T">
    ///     Type of data from the source to be processed.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     Type that contains context about the data from the source.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Type that will be used to key data from the source for distribution.
    /// </typeparam>
    public interface ISourceProcessingSession<T, TContext, TKey>
        : ISourceParserDescriptor,
          ISourceDataCookerRegistrar<T, TContext, TKey>,
          ISourceDataProcessor<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Called to process the source.
        /// </summary>
        /// <param name="logger">
        ///     Log messages.
        /// </param>
        /// <param name="progress">
        ///     Progress updates.
        /// </param>
        /// <param name="cancellationToken">
        ///     Cancellation token.
        /// </param>
        void ProcessSource(ILogger logger, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
