// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    internal class SourceProcessingSessionFactory
        : ISourceSessionFactory
    {
        public ISourceProcessingSession<T, TContext, TKey> CreateSourceSession<T, TContext, TKey>(
            ICustomDataProcessorWithSourceParser<T, TContext, TKey> customDataProcessor) 
            where T : IKeyedDataType<TKey>
        {
            Guard.NotNull(customDataProcessor, nameof(customDataProcessor));

            var sourceParser = customDataProcessor.SourceParser;
            if (sourceParser == null)
            {
                return null;
            }

            return new SourceProcessingSession<T, TContext, TKey>(sourceParser);
        }
    }
}
