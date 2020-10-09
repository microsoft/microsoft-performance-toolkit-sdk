// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     This interface exposes a method to process data from a source.
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
    public interface ISourceDataProcessor<in T, in TContext, in TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Processes data from the source.
        /// </summary>
        /// <param name="data">
        ///     The data from the source to be processed.
        /// </param>
        /// <param name="context">
        ///     Additional information to assist processing.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signal used to signify that the caller wants to cancel the operation.
        /// </param>
        /// <returns>
        ///     The result from processing the data.
        /// </returns>
        DataProcessingResult ProcessDataElement(T data, TContext context, CancellationToken cancellationToken);
    }
}
