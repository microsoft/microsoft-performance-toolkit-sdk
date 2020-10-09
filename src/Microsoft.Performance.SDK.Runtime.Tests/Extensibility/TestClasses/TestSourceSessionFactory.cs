// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestSourceSessionFactory
        : ISourceSessionFactory
    {
        public ISourceProcessingSession<T, TContext, TKey> CreateSourceSession<T, TContext, TKey>(
            ICustomDataProcessorWithSourceParser<T, TContext, TKey> customDataProcessor)
            where T : IKeyedDataType<TKey>
        {
            return new TestSourceProcessingSession<T, TContext, TKey>() { CustomDataProcessor = customDataProcessor };
        }
    }
}
