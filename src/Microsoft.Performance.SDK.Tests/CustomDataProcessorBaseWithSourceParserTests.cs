// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class CustomDataProcessorBaseWithSourceParserTests
    {
        [Table]
        public static class TestTable1
        {
            public static readonly string SourceParserId = "SourceId1";

            public static readonly DataCookerPath RequiredDataCooker =
                DataCookerPath.ForSource(SourceParserId, "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{C2AC873F-B88E-46A1-A08B-0CDF1D0C9F18}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: false,
                    isInternalTable: true,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker })
            { Type = typeof(TestTable1) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [Table]
        public static class NoBuildActionInternalTestTable1
        {
            public static readonly string SourceParserId = "SourceId1";

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{CFBE73E8-DBC6-4A76-9F98-B4F2C1D4816D}"),
                    "Test Table without a build method.",
                    "This table has no required data extensions",
                    "Other",
                    isMetadataTable: false,
                    isInternalTable: true)
            { Type = typeof(NoBuildActionInternalTestTable1) };
        }

        [Table]
        public static class NoBuildActionInternalTestTable2
        {
            public static readonly string SourceParserId = "SourceId1";

            public static readonly DataCookerPath RequiredDataCooker =
                DataCookerPath.ForSource(SourceParserId, "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{87C11CDF-5D35-49AC-8FFD-6E34D1930A35}"),
                    "Test Table without a build method",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: false,
                    isInternalTable: true,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker })
            { Type = typeof(NoBuildActionInternalTestTable2) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        // This table is marked as internal and references multiple source parser.
        [Table]
        public static class InvalidInternalTestTable
        {
            public static readonly DataCookerPath RequiredDataCooker1 =
                DataCookerPath.ForSource("SourceId1", "CookerId1");

            public static readonly DataCookerPath RequiredDataCooker2 =
                DataCookerPath.ForSource("SourceId2", "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{F4C87E7F-C41D-4323-AB63-B32E2F7DF2E0}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: false,
                    isInternalTable: true,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker1, RequiredDataCooker2 })
            { Type = typeof(InvalidInternalTestTable) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [TestMethod]
        [UnitTest]
        public void InternalTableIsEnabled()
        {
            var sourceCooker = new TestSourceDataCooker()
            { Path = TestTable1.RequiredDataCooker };

            var sourceCookerReference = new TestSourceDataCookerReference(false)
            {
                Path = sourceCooker.Path,
                availability = DataExtensionAvailability.Available,
                createInstance = () => sourceCooker,
            };

            var internalTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            internalTables.Add(TestTable1.TableDescriptor, TestTable1.BuildTable);

            var extensions = new TestDataExtensionRepository();
            extensions.sourceCookersByPath.Add(sourceCookerReference.Path, sourceCookerReference);

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId, internalTables, extensions);

            // Make sure the table is marked as registered in the base class
            Assert.AreEqual(1, cdp.EnabledTableDescriptors.Count);
            Assert.AreEqual(typeof(TestTable1), cdp.EnabledTableDescriptors.First().Type);

            // Make sure that the table was registered with the processor's extensibility support.
            var enabledTables = cdp.ExtensibilitySupport.GetEnabledInternalTables();
            Assert.AreEqual(1, enabledTables.Count());
            Assert.AreEqual(typeof(TestTable1), enabledTables.First().Type);
        }

        [TestMethod]
        [UnitTest]
        public void InternalTableWithNoBuildMethodIsEnabled()
        {
            var sourceCooker = new TestSourceDataCooker()
            { Path = TestTable1.RequiredDataCooker };

            var sourceCookerReference = new TestSourceDataCookerReference(false)
            {
                Path = sourceCooker.Path,
                availability = DataExtensionAvailability.Available,
                createInstance = () => sourceCooker,
            };

            var internalTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            internalTables.Add(TestTable1.TableDescriptor, TestTable1.BuildTable);
            internalTables.Add(NoBuildActionInternalTestTable1.TableDescriptor, null);
            internalTables.Add(NoBuildActionInternalTestTable2.TableDescriptor, null);

            var extensions = new TestDataExtensionRepository();
            extensions.sourceCookersByPath.Add(sourceCookerReference.Path, sourceCookerReference);

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId, internalTables, extensions);

            // Make sure the table is marked as registered in the base class
            Assert.AreEqual(3, cdp.EnabledTableDescriptors.Count);
            Assert.IsTrue(cdp.EnabledTableDescriptors.Select(t => t.Type).Contains(typeof(TestTable1)));
            Assert.IsTrue(cdp.EnabledTableDescriptors.Select(t => t.Type).Contains(typeof(NoBuildActionInternalTestTable1)));
            Assert.IsTrue(cdp.EnabledTableDescriptors.Select(t => t.Type).Contains(typeof(NoBuildActionInternalTestTable2)));

            // NoBuildActionInternalTestTable1 doesn't have any required cookers, so it won't show up here
            var enabledTables = cdp.ExtensibilitySupport.GetEnabledInternalTables();
            Assert.AreEqual(2, enabledTables.Count());
            Assert.IsTrue(enabledTables.Select(t => t.Type).Contains(typeof(TestTable1)));
            Assert.IsTrue(enabledTables.Select(t => t.Type).Contains(typeof(NoBuildActionInternalTestTable2)));
        }

        [TestMethod]
        [UnitTest]
        public void InternalTableWithExternalCookerRequired()
        {
            // This should fail to be added because internal tables cannot reference external data cookers.

            var sourceCooker = new TestSourceDataCooker()
            { Path = TestTable1.RequiredDataCooker };

            var sourceCookerReference = new TestSourceDataCookerReference(false)
            {
                Path = sourceCooker.Path,
                availability = DataExtensionAvailability.Available,
                createInstance = () => sourceCooker,
            };

            var internalTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();

            internalTables.Add(TestTable1.TableDescriptor, TestTable1.BuildTable);

            var extensions = new TestDataExtensionRepository();
            extensions.sourceCookersByPath.Add(sourceCookerReference.Path, sourceCookerReference);

            // create a custom data processor with a different source parser than the table references
            var sut = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId + "_X", internalTables, extensions);

            // We expect a verbose message if the table cannot be enabled.
            Assert.AreEqual(1, sut.TestLogger.VerboseCalls.Count);

            // Make sure the table is marked as registered in the base class
            Assert.AreEqual(0, sut.EnabledTableDescriptors.Count);

            // Make sure that the table was registered with the processor's extensibility support.
            var enabledTables = sut.ExtensibilitySupport.GetEnabledInternalTables();
            Assert.AreEqual(0, enabledTables.Count());
        }

        [TestMethod]
        [UnitTest]
        public void InternalTableRequiresMultipleSourceParsers()
        {
            // This should fail to be added because internal tables cannot reference multiple source parsers.
            // An internal table is handled internally by a single custom data processor and the processor
            // can't have access to data outside that processor.
            //

            var sourceCooker1 = new TestSourceDataCooker()
            { Path = InvalidInternalTestTable.RequiredDataCooker1 };

            var sourceCookerReference1 = new TestSourceDataCookerReference(false)
            {
                Path = sourceCooker1.Path,
                availability = DataExtensionAvailability.Available,
                createInstance = () => sourceCooker1,
            };

            var sourceCooker2 = new TestSourceDataCooker()
            { Path = InvalidInternalTestTable.RequiredDataCooker2 };

            var sourceCookerReference2 = new TestSourceDataCookerReference(false)
            {
                Path = sourceCooker2.Path,
                availability = DataExtensionAvailability.Available,
                createInstance = () => sourceCooker2,
            };

            var internalTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();

            internalTables.Add(InvalidInternalTestTable.TableDescriptor, InvalidInternalTestTable.BuildTable);

            var extensions = new TestDataExtensionRepository();
            extensions.sourceCookersByPath.Add(sourceCookerReference1.Path, sourceCookerReference1);
            extensions.sourceCookersByPath.Add(sourceCookerReference2.Path, sourceCookerReference2);

            // create a custom data processor with a different source parser than the table references
            var sut = TestCustomDataProcessor.CreateTestCustomDataProcessor("SourceParserId", internalTables, extensions);

            // We expect a warning message if the table is invalid
            Assert.AreEqual(1, sut.TestLogger.WarnCalls.Count);

            // Make sure the table is marked as registered in the base class
            Assert.AreEqual(0, sut.EnabledTableDescriptors.Count);

            // Make sure that the table was registered with the processor's extensibility support.
            var enabledTables = sut.ExtensibilitySupport.GetEnabledInternalTables();
            Assert.AreEqual(0, enabledTables.Count());
        }
    }
}
