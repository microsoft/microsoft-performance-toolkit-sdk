// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     Implementors are responsible for creating <see cref="ISourceProcessingSession{T,TContext,TKey}"/>.
    /// </summary>
    public interface ISourceSessionFactory
    {
        /// <summary>
        ///     Creates a source session associated with the given custom data processor.
        /// </summary>
        /// /// <typeparam name="T">
        ///     Type of data from the source to be processed.
        /// </typeparam>
        /// <typeparam name="TContext">
        ///     Type that contains context about the data from the source.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Type that will be used to key data from the source for distribution.
        /// </typeparam>
        /// <param name="customDataProcessor">
        ///     Custom data processor.
        /// </param>
        /// <returns>
        ///     A source session associated with the custom data processor.
        /// </returns>
        ISourceProcessingSession<T, TContext, TKey> CreateSourceSession<T, TContext, TKey>(
            ICustomDataProcessorWithSourceParser<T, TContext, TKey> customDataProcessor) where T : IKeyedDataType<TKey>;
    }
}
