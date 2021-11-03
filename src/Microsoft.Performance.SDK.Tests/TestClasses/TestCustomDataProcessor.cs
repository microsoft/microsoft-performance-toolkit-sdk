// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Tests.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestCustomDataProcessor
        : CustomDataProcessorWithSourceParser<TestRecord, TestParserContext, int>
    {
        public static TestCustomDataProcessor CreateTestCustomDataProcessor(
            string sourceParserId = "TestSourceParser")
        {
            return CreateTestCustomDataProcessor(
                sourceParserId,
                new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>());
        }

        public static TestCustomDataProcessor CreateTestCustomDataProcessor(
            string sourceParserId,
            Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> metadataTables,
            TestDataExtensionRepository extensionRepo = null)
        {
            var sourceParser = new TestSourceParser() { Id = sourceParserId };
            var applicationEnvironment = new TestApplicationEnvironment();
            var processorEnvironment = CreateProcessorEnvironment();
            var dataExtensionRepo = extensionRepo ?? new TestDataExtensionRepository();

            CustomDataProcessorExtensibilitySupport extensibilitySupport = null;
            var compositeCookers = new ProcessingSystemCompositeCookers(dataExtensionRepo);

            applicationEnvironment.SourceSessionFactory = new TestSourceSessionFactory();
            processorEnvironment.CreateDataProcessorExtensibilitySupportFunc = (processor) =>
            {
                extensibilitySupport = new CustomDataProcessorExtensibilitySupport(
                    processor,
                    dataExtensionRepo,
                    compositeCookers);

                return extensibilitySupport;
            };

            // this will create the CustomDataProcessorExtensibilitySupport
            var cdp = new TestCustomDataProcessor(
                sourceParser,
                ProcessorOptions.Default,
                applicationEnvironment,
                processorEnvironment,
                metadataTables,
                metadataTables.Select(kvp => kvp.Key));

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

        public List<ISourceDataCooker<TestRecord, TestParserContext, int>> EnabledCookers { get; }
            = new List<ISourceDataCooker<TestRecord, TestParserContext, int>>();
        protected override void OnDataCookerEnabled(ISourceDataCooker<TestRecord, TestParserContext, int> sourceDataCooker)
        {
            Debug.Assert(sourceDataCooker != null);
            EnabledCookers.Add(sourceDataCooker);
        }

        public CustomDataProcessorExtensibilitySupport ExtensibilitySupport { get; set; }

        public TestDataExtensionRepository ExtensionRepository { get; set; }

        public ReadOnlyHashSet<TableDescriptor> EnabledTableDescriptors => this.EnabledTables;

        public TestLogger TestLogger => this.Logger as TestLogger;

        private static TestProcessorEnvironment CreateProcessorEnvironment()
        {
            var processorEnvironment = new TestProcessorEnvironment();
            processorEnvironment.TestLoggers.Add(typeof(TestCustomDataProcessor), new TestLogger());
            return processorEnvironment;
        }
    }
}
