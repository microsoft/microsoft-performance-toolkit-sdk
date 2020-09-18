// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     Implemented by a source to control processing.
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
    public interface ISourceProcessor<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     This is called by the runtime before a call to <see cref="ProcessSource"/>, giving the source
        ///     parser some extra data. Some parsers may be able to optimize source processing knowing which
        ///     data elements (identified by <paramref name="requestedDataKeys"/>) will be needed by source cookers.
        /// </summary>
        /// <param name="allEventsConsumed">
        ///     When this is true, all events will be processed while parsing the data source. When false, 
        ///     only events identified by <paramref name="requestedDataKeys"/> will be processed.
        /// </param>
        /// <param name="requestedDataKeys">
        ///     The set of data keys specifically required by source data cookers that will take part in
        ///     the upcoming call to <see cref="ProcessSource"/>.
        /// </param>
        void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<TKey> requestedDataKeys);

        /// <summary>
        ///     The source parser should begin processing the source data set when this is called.
        /// </summary>
        /// <param name="dataProcessor">
        ///     Each data element will be passed to this by calling <c>dataProcessor.ProcessDataElement</c>.
        ///     </param>
        /// <param name="logger">
        ///     Provides a way for the source parser to log status messages.
        /// </param>
        /// <param name="progress">
        ///     Used to indicate progress parsing the source.
        /// </param>
        /// <param name="cancellationToken">
        ///     Used to request the operation be canceled.
        /// </param>
        void ProcessSource(ISourceDataProcessor<T, TContext, TKey> dataProcessor, ILogger logger, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
