// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    /// <inheritdoc />
    public class TestSourceParser
        : ISourceParser<TestRecord, TestParserContext, int>
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public Type DataElementType { get; set; }

        /// <inheritdoc />
        public Type DataContextType { get; set; }

        /// <inheritdoc />
        public Type DataKeyType { get; set; }

        /// <inheritdoc />
        public int MaxSourceParseCount { get; set; }

        public bool ReceivedAllEventsConsumed { get; set; }
        public IReadOnlyCollection<int> RequestedDataKeys { get; set; }
        /// <inheritdoc/>
        public virtual void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<int> requestedDataKeys)
        {
            this.ReceivedAllEventsConsumed = allEventsConsumed;
            this.RequestedDataKeys = requestedDataKeys;
        }

        /// <inheritdoc />
        public void ProcessSource(
            ISourceDataProcessor<TestRecord, TestParserContext, int> dataProcessor, 
            ILogger logger, 
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            if (this.TestRecords == null)
            {
                return;
            }

            foreach (var testRecord in this.TestRecords)
            {
                dataProcessor.ProcessDataElement(testRecord, new TestParserContext(), cancellationToken);
            }

            progress.Report(100);
        }

        /// <inheritdoc />
        public DataSourceInfo DataSourceInfo { get; set; }

        public IEnumerable<TestRecord> TestRecords { get; set; }
    }
}
