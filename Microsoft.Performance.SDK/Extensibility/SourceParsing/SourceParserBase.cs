// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     This implements some basic elements of <see cref="ISourceParser{T,TContext,TKey}"/>.
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
    public abstract class SourceParserBase<T, TContext, TKey>
        : ISourceParser<T, TContext, TKey> 
          where T : IKeyedDataType<TKey>
    {
        /// <inheritdoc/>
        public Type DataElementType => typeof(T);

        /// <inheritdoc/>
        public Type DataContextType => typeof(TContext);

        /// <inheritdoc/>
        public Type DataKeyType => typeof(TKey);

        /// <inheritdoc/>
        public virtual int MaxSourceParseCount => SourceParsingConstants.UnlimitedPassCount;

        /// <inheritdoc/>
        public abstract string Id { get; }

        /// <inheritdoc/>
        public virtual void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<TKey> requestedDataKeys)
        {
            // the default implementation does nothing with this data
        }

        /// <inheritdoc/>
        public abstract void ProcessSource(
            ISourceDataProcessor<T, TContext, TKey> dataProcessor, 
            ILogger logger,
            IProgress<int> progress, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract DataSourceInfo DataSourceInfo { get; }
    }
}
