// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Tests.DataTypes;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestCustomDataProcessor
        : CustomDataProcessorWithSourceParser<TestRecord, TestParserContext, int>
    {
        public static TestCustomDataProcessor CreateTestCustomDataProcessor(
            string sourceParserId = "TestSourceParser")
        {
            return CreateTestCustomDataProcessor(
                sourceParserId);
        }

        public static TestCustomDataProcessor CreateTestCustomDataProcessor(
            string sourceParserId,
            TestDataExtensionRepository extensionRepo = null)
        {
            var sourceParser = new TestSourceParser() { Id = sourceParserId };
            var applicationEnvironment = new TestApplicationEnvironment();
            var processorEnvironment = CreateProcessorEnvironment();
            var dataExtensionRepo = extensionRepo ?? new TestDataExtensionRepository();

            applicationEnvironment.SourceSessionFactory = new TestSourceSessionFactory();

            // this will create the CustomDataProcessorExtensibilitySupport
            var cdp = new TestCustomDataProcessor(
                sourceParser,
                ProcessorOptions.Default,
                applicationEnvironment,
                processorEnvironment);

            cdp.ExtensionRepository = dataExtensionRepo;

            return cdp;
        }

        public TestCustomDataProcessor(
            ISourceParser<TestRecord, TestParserContext, int> sourceParser,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment)
            : base(sourceParser, options, applicationEnvironment, processorEnvironment)
        {
        }

        public List<ISourceDataCooker<TestRecord, TestParserContext, int>> EnabledCookers { get; }
            = new List<ISourceDataCooker<TestRecord, TestParserContext, int>>();
        protected override void OnDataCookerEnabled(ISourceDataCooker<TestRecord, TestParserContext, int> sourceDataCooker)
        {
            Debug.Assert(sourceDataCooker != null);
            EnabledCookers.Add(sourceDataCooker);
        }

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
