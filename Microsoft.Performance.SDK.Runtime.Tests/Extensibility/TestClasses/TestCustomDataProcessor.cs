// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestCustomDataProcessor
        : CustomDataProcessorBaseWithSourceParser<TestRecord, TestParserContext, int>
    {
        internal static TestCustomDataProcessor CreateTestCustomDataProcessor(
            string sourceParserId = "TestSourceParser")
        {
            var sourceParser = new TestSourceParser() { Id = sourceParserId };
            var applicationEnvironment = new TestApplicationEnvironment();
            var processorEnvironment = new TestProcessorEnvironment();
            var dataExtensionRepo = new TestDataExtensionRepository();

            CustomDataProcessorExtensibilitySupport extensibilitySupport = null;

            applicationEnvironment.SourceSessionFactory = new TestSourceSessionFactory();
            processorEnvironment.CreateDataProcessorExtensibilitySupportFunc = (processor) =>
            {
                extensibilitySupport = new CustomDataProcessorExtensibilitySupport(processor, dataExtensionRepo);
                return extensibilitySupport;
            };

            // this will create the CustomDataProcessorExtensibilitySupport
            var cdp = new TestCustomDataProcessor(
                sourceParser,
                ProcessorOptions.Default,
                applicationEnvironment,
                processorEnvironment,
                new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(),
                new List<TableDescriptor>());

            Assert.IsNotNull(extensibilitySupport);

            cdp.ExtensibilitySupport = extensibilitySupport;
            cdp.ExtensionRepository = dataExtensionRepo;

            return cdp;
        }

        public TestCustomDataProcessor(
            ISourceParser<TestRecord, TestParserContext, int> sourceParser, 
            ProcessorOptions options, 
            IApplicationEnvironment applicationEnvironment, 
            IProcessorEnvironment processorEnvironment, 
            IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping, 
            IEnumerable<TableDescriptor> metadataTables) 
            : base(sourceParser, options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
        }

        public CustomDataProcessorExtensibilitySupport ExtensibilitySupport { get; set;}

        public TestDataExtensionRepository ExtensionRepository { get; set; }
    }
}
