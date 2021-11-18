// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides access to data from a <see cref="ICustomDataProcessor"/>'s source parser cookers.
    /// </summary>
    public interface ISourceParserRetrieval
          : ICookedDataRetrieval
    {
        /// <summary>
        ///     Source parser identifier
        /// </summary>
        string SourceParserId { get; }
    }

    /// <summary>
    ///     Wraps the ICustomDataProcessor with additional functionality required to operate with a
    ///     source parser and data extensions.
    /// </summary>
    public interface ICustomDataProcessorWithSourceParser
        : ICustomDataProcessor,
          ISourceParserRetrieval
    {
        /// <summary>
        ///     Enables a source data cooker, causing it to take part in processing the source data.
        /// </summary>
        /// <param name="sourceDataCookerFactory">
        ///     A factory for creating the source data cooker to enable.
        /// </param>
        void EnableCooker(ISourceDataCookerFactory sourceDataCookerFactory);
    }

    /// <summary>
    ///     Wraps ICustomDataProcessorWithSourceParser as a generic that is specific to a give source parser.
    /// </summary>
    /// <typeparam name="T">
    ///     Type of data from the source to be processed.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     Type that contains context about the data from the source.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Type that will be used to identify data from the source that is relevant to this extension.
    /// </typeparam>
    public interface ICustomDataProcessorWithSourceParser<T, TContext, TKey>
        : ICustomDataProcessorWithSourceParser
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     The source parser responsible for parsing the source and handing data to the
        ///     <see cref="ISourceDataProcessor{T, TContext, TKey}"/> for further processing.
        /// </summary>
        ISourceParser<T, TContext, TKey> SourceParser { get; }

        /// <summary>
        ///     The source session, responsible for processing the source through the source
        ///     parser, and distributing data to source data cookers as appropriate.
        /// </summary>
        ISourceProcessingSession<T, TContext, TKey> SourceProcessingSession { get; }
    }
}
