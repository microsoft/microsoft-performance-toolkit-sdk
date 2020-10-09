// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     A source parser is responsible for parsing a source, interacting with an
    ///     <see cref="ISourceDataProcessor&lt;T, TContext, TKey&gt;"/> to distribute data elements from the source,
    ///     and exposing source related data.
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
    public interface ISourceParser<T, TContext, TKey>
        : ISourceParserDescriptor,
          ISourceProcessor<T, TContext, TKey>
          where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Gets information about a data source, including the
        ///     time range encompassed by the data source.
        /// </summary>
        DataSourceInfo DataSourceInfo { get; }
    }
}
