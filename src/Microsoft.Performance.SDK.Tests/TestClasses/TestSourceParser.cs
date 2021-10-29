// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Tests.DataTypes;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;

namespace Microsoft.Performance.SDK.Tests.TestClasses
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
        public int MaxSourceParseCount => SourceParsingConstants.UnlimitedPassCount;

        public bool ReceivedAllEventsConsumed { get; set; }
        public IReadOnlyCollection<int> RequestedDataKeys { get; set; }
        /// <inheritdoc/>
        public virtual void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<int> requestedDataKeys)
        {
            ReceivedAllEventsConsumed = allEventsConsumed;
            RequestedDataKeys = requestedDataKeys;
        }

        /// <inheritdoc />
        public void ProcessSource(
            ISourceDataProcessor<TestRecord, TestParserContext, int> dataProcessor,
            ILogger logger,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            if (TestRecords == null)
            {
                return;
            }

            var totalNumTestRecords = this.TestRecords.Count();
            for (int testRecordIdx = 0; testRecordIdx < totalNumTestRecords; testRecordIdx++)
            {
                var testRecord = this.TestRecords.ElementAt(testRecordIdx);
                dataProcessor.ProcessDataElement(testRecord, new TestParserContext(), cancellationToken);

                progress.Report(100 * (testRecordIdx + 1) / totalNumTestRecords);
            }
        }

        /// <inheritdoc />
        public DataSourceInfo DataSourceInfo { get; set; }

        public IEnumerable<TestRecord> TestRecords { get; set; }
    }
}
