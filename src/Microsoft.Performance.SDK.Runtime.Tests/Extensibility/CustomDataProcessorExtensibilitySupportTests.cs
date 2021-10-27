// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.Exceptions;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class CustomDataProcessorExtensibilitySupportTests
    {
        [Table]
        public class TestTable1
        {
            public static readonly string SourceParserId = "SourceId1";

            public static readonly DataCookerPath RequiredDataCooker =
                DataCookerPath.ForSource(SourceParserId, "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{C2AC873F-B88E-46A1-A08B-0CDF1D0C9F18}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: true,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker })
            { Type = typeof(TestTable1) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [Table]
        public class TestTable2
        {
            public static readonly string SourceParserId1 = "SourceId1";
            public static readonly string SourceParserId2 = "SourceId2";

            public static readonly DataCookerPath RequiredDataCooker1 =
                DataCookerPath.ForSource(SourceParserId1, "CookerId1");

            public static readonly DataCookerPath RequiredDataCooker2 =
                DataCookerPath.ForSource(SourceParserId2, "CookerId1");

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{DE4A732B-0CCE-4A74-9266-291BCA6E0F6C}"),
                    "Test table with required data cookers",
                    "This table has required data extensions from multiple parsers",
                    "Other",
                    isMetadataTable: false,
                    requiredDataCookers: new List<DataCookerPath> { RequiredDataCooker1, RequiredDataCooker2 })
            { Type = typeof(TestTable2) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [Table]
        public class TestTable3
        {
            public static readonly string SourceParserId = "SourceId2";

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{BCF87F9D-E7D9-438D-8799-179AC39DF3FD}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    isMetadataTable: true,
                    requiredDataCookers: new List<DataCookerPath> { DataCookerPath.ForSource(SourceParserId, "CookerId1") })
            { Type = typeof(TestTable3) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        private void EnableTableWithNoRequiredDataExtensionsImpl(
            Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();

            var tableDescriptorWithNoRequirements = new TableDescriptor(
                Guid.Parse("{94267E78-5B18-4FA7-A6C0-8E21B06DF65A}"),
                "Simple Test Table",
                "This table has no required data extensions");

            // Nothing to do to prepare the processor for this table, but it's not an error.

            enableTable(cdp, tableDescriptorWithNoRequirements);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableTableWithNoRequiredDataExtensions()
        {
            EnableTableWithNoRequiredDataExtensionsImpl(
                (cdp, table) => Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableTableWithNoRequiredDataExtensions()
        {
            EnableTableWithNoRequiredDataExtensionsImpl(
                (cdp, table) => cdp.ExtensibilitySupport.EnableTable(table));
        }

        private void EnableTableWithMissingRequirementsImpl(
             Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            // the table won't be available with a missing dependency

            enableTable(cdp, TestTable1.TableDescriptor);

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable3.TableDescriptor);
            Assert.IsNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableTableWithMissingRequirements()
        {
            EnableTableWithMissingRequirementsImpl(
                (cdp, table) => Assert.IsFalse(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableTableWithMissingRequirements()
        {
            EnableTableWithMissingRequirementsImpl(
                (cdp, table) => Assert.ThrowsException<ExtensionTableException>(
                    () => cdp.ExtensibilitySupport.EnableTable(table)));
        }

        private void EnableAfterFinalizeImpl(
            Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            cdp.ExtensibilitySupport.FinalizeTables();

            enableTable(cdp, TestTable1.TableDescriptor);

            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor);
            Assert.IsNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableAfterFinalize()
        {
            EnableAfterFinalizeImpl(
                (cdp, table) => Assert.IsFalse(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableAfterFinalize()
        {
            EnableAfterFinalizeImpl(
                (cdp, table) => Assert.ThrowsException<InvalidOperationException>(
                    () => cdp.ExtensibilitySupport.EnableTable(table)));
        }

        private void EnableValidTableImpl(
            Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = TestTable1.RequiredDataCooker };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = TestTable1.RequiredDataCooker, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(TestTable1.RequiredDataCooker, sourceCookerReference);

            enableTable(cdp, TestTable1.TableDescriptor);

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(1, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor);
            Assert.IsNotNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableValidTable()
        {
            EnableValidTableImpl(
                (cdp, table) => Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableValidTable()
        {
            EnableValidTableImpl(
                (cdp, table) => cdp.ExtensibilitySupport.EnableTable(table));
        }

        private void EnableExternalTableImpl(Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker1 = new TestSourceDataCooker() { Path = TestTable2.RequiredDataCooker1 };
            var sourceCookerReference1 = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker1.Path, createInstance = () => sourceCooker1 };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCookerReference1.Path, sourceCookerReference1);

            var sourceCooker2 = new TestSourceDataCooker() { Path = TestTable2.RequiredDataCooker2 };
            var sourceCookerReference2 = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker2.Path, createInstance = () => sourceCooker2 };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCookerReference2.Path, sourceCookerReference2);

            enableTable(cdp, TestTable2.TableDescriptor);

            cdp.ExtensibilitySupport.FinalizeTables();

            // Table2 is an external table, so there shouldn't be any source cookers reported here.
            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            // Table2 is an external table, so there's no IDataExtensionRetrieval created by this data processor
            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable2.TableDescriptor);
            Assert.IsNull(dataRetrieval);

            // The required source cooker from cdp should have been enabled
            Assert.AreEqual(1, cdp.EnabledCookers.Count);
            Assert.AreEqual(cdp.EnabledCookers[0], sourceCooker1);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableExternalTable()
        {
            EnableExternalTableImpl(
                (cdp, table) => Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableExternalTable()
        {
            EnableExternalTableImpl(
                (cdp, table) => cdp.ExtensibilitySupport.EnableTable(table));
        }

        private void EnableTableUnavailableDataCookerImpl(
            Action<TestCustomDataProcessor, TableDescriptor> enableTable)
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker1 = new TestSourceDataCooker() { Path = TestTable2.RequiredDataCooker1 };
            var sourceCookerReference1 = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker1.Path, createInstance = () => sourceCooker1 };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCookerReference1.Path, sourceCookerReference1);

            // setup this cooker as unavailable
            var sourceCooker2 = new TestSourceDataCooker() { Path = TestTable2.RequiredDataCooker2 };
            var sourceCookerReference2 = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Error, Path = sourceCooker2.Path, createInstance = () => null };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCookerReference2.Path, sourceCookerReference2);

            enableTable(cdp, TestTable2.TableDescriptor);

            cdp.ExtensibilitySupport.FinalizeTables();

            // No tables should be enabled
            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable2.TableDescriptor);
            Assert.IsNull(dataRetrieval);

            Assert.AreEqual(0, cdp.EnabledCookers.Count);
        }

        [UnitTest]
        [TestMethod]
        public void TryEnableTableUnavailableDataCooker()
        {
            EnableTableUnavailableDataCookerImpl(
                (cdp, table) => Assert.IsFalse(cdp.ExtensibilitySupport.TryEnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void EnableTableUnavailableDataCooker()
        {
            EnableTableUnavailableDataCookerImpl(
                (cdp, table) => Assert.ThrowsException<ExtensionTableException>(
                    () => cdp.ExtensibilitySupport.EnableTable(table)));
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTablesWithSameSourceParser()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = DataCookerPath.ForSource(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(TestTable1.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(1, requiredCookers.Count);
            Assert.AreEqual(sourceCooker.Path, requiredCookers.First());

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor);
            Assert.IsNotNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTablesFromVariedParsers()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = DataCookerPath.ForSource(TestTable3.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            // This requires a source cooker from a different cdp, so it should fail to be added
            Assert.IsFalse(cdp.ExtensibilitySupport.TryEnableTable(TestTable3.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable3.TableDescriptor);
            Assert.IsNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTwiceThrows()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = DataCookerPath.ForSource(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(TestTable1.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();
            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.FinalizeTables());
        }

        [UnitTest]
        [TestMethod]
        public void GetAllRequiredSourceDataCookersWithoutFinalizeThrows()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = DataCookerPath.ForSource(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(TestTable1.TableDescriptor));

            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.GetRequiredSourceDataCookers());
        }

        [UnitTest]
        [TestMethod]
        public void GetDataExtensionRetrievalWithoutFinalizeThrows()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = DataCookerPath.ForSource(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path, createInstance = () => sourceCooker };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.TryEnableTable(TestTable1.TableDescriptor));

            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor));
        }

        [TestMethod]
        [UnitTest]
        public void QueryInvalidSourceParserIdThrows()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            Assert.ThrowsException<ArgumentException>(
                () => cdp.QueryOutput(DataCookerPath.ForSource("InvalidParserId", "RandomeCookerName"), "DataId"));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryInvalidSourceParserIdFails()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            var success = cdp.TryQueryOutput(
                DataCookerPath.ForSource("InvalidParserId", "RandomeCookerName"),
                "DataId",
                out var result);

            Assert.IsFalse(success);
            Assert.AreEqual(default, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryInvalidSourceParserIdGenericThrows()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            Assert.ThrowsException<ArgumentException>(
                () => cdp.QueryOutput<int>(DataCookerPath.ForSource("InvalidParserId", "RandomeCookerName"), "DataId"));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryInvalidSourceParserIdGenericFails()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            var success = cdp.TryQueryOutput(
                DataCookerPath.ForSource("InvalidParserId", "RandomCookerName"),
                "DataId",
                out bool result);

            Assert.IsFalse(success);
            Assert.AreEqual(default, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryOutput()
        {
            string sourceParserId = "TestSourceParser";
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(sourceParserId);

            var dataCookerPath = DataCookerPath.ForSource(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = sourceCooker.Path,
                createInstance = () => sourceCooker,
            };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);
            cdp.EnableCooker(sourceCookerReference);

            var addFuncAsObject = cdp.QueryOutput(dataCookerPath, nameof(TestCookedDataReflector.AddFunc));

            int result = ((Func<int, int, int>)addFuncAsObject)(0, 1);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryOutputByType()
        {
            string sourceParserId = "TestSourceParser";
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(sourceParserId);

            var dataCookerPath = DataCookerPath.ForSource(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = sourceCooker.Path,
                createInstance = () => sourceCooker,
            };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);
            cdp.EnableCooker(sourceCookerReference);

            var addFuncAsObject = cdp.QueryOutput<Func<int, int, int>>(
                dataCookerPath,
                nameof(TestCookedDataReflector.AddFunc));

            int result = addFuncAsObject(5, 10);
            Assert.AreEqual(15, result);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryOutput()
        {
            string sourceParserId = "TestSourceParser";
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(sourceParserId);

            var dataCookerPath = DataCookerPath.ForSource(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = sourceCooker.Path,
                createInstance = () => sourceCooker,
            };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);
            cdp.EnableCooker(sourceCookerReference);

            bool success = cdp.TryQueryOutput(
                dataCookerPath,
                nameof(TestCookedDataReflector.HasData),
                out var hasDataAsObject);

            Assert.IsTrue(success);
            Assert.AreEqual(cookedDataReflector.HasData, (bool)hasDataAsObject);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryOutputByType()
        {
            string sourceParserId = "TestSourceParser";
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(sourceParserId);

            var dataCookerPath = DataCookerPath.ForSource(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestRuntimeSourceCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = sourceCooker.Path,
                createInstance = () => sourceCooker,
            };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);
            cdp.EnableCooker(sourceCookerReference);

            var success = cdp.TryQueryOutput<bool>(
                dataCookerPath,
                nameof(TestCookedDataReflector.HasData),
                out var result);

            Assert.IsTrue(success);
            Assert.AreEqual(cookedDataReflector.HasData, result);
        }
    }
}
