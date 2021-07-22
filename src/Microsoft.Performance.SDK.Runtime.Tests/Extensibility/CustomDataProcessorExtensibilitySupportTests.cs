// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
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

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{C2AC873F-B88E-46A1-A08B-0CDF1D0C9F18}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    true,
                    requiredDataCookers: new List<DataCookerPath> { new DataCookerPath(SourceParserId, "CookerId1") })
            { Type = typeof(TestTable1) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [Table]
        public class TestTable2
        {
            public static readonly string SourceParserId = "SourceId2";

            public static TableDescriptor TableDescriptor = new TableDescriptor(
                    Guid.Parse("{BCF87F9D-E7D9-438D-8799-179AC39DF3FD}"),
                    "Test Table with Requirements",
                    "This table has required data extensions",
                    "Other",
                    true,
                    requiredDataCookers: new List<DataCookerPath> { new DataCookerPath(SourceParserId, "CookerId1") })
            { Type = typeof(TestTable2) };

            public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataRetrieval)
            {
            }
        }

        [UnitTest]
        [TestMethod]
        public void AddTableWithNoRequiredDataExtensions()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();

            var tableDescriptorWithNoRequirements = new TableDescriptor(
                Guid.Parse("{94267E78-5B18-4FA7-A6C0-8E21B06DF65A}"),
                "Simple Test Table",
                "This table has no required data extensions");

            Assert.IsFalse(cdp.ExtensibilitySupport.AddTable(tableDescriptorWithNoRequirements));
        }

        [UnitTest]
        [TestMethod]
        public void AddTableWithRequiredDataExtensions()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));
        }

        [UnitTest]
        [TestMethod]
        public void AddTableWithMissingRequirements()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));

            // Not adding the source data cooker that is required by the table, so we expect that FinalizeTables will
            // remove the table.
            //

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetAllRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable2.TableDescriptor);
            Assert.IsNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTablesWithSameSourceParser()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = new DataCookerPath(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestSourceDataCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetAllRequiredSourceDataCookers();
            Assert.AreEqual(1, requiredCookers.Count);
            Assert.AreEqual(sourceCooker.Path, requiredCookers.First());

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor);
            Assert.IsNotNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTablesFromVariedParsers()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = new DataCookerPath(TestTable2.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestSourceDataCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable2.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();

            var requiredCookers = cdp.ExtensibilitySupport.GetAllRequiredSourceDataCookers();
            Assert.AreEqual(0, requiredCookers.Count);

            var dataRetrieval = cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable2.TableDescriptor);
            Assert.IsNull(dataRetrieval);
        }

        [UnitTest]
        [TestMethod]
        public void FinalizeTwiceThrows()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = new DataCookerPath(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestSourceDataCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));

            cdp.ExtensibilitySupport.FinalizeTables();
            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.FinalizeTables());
        }

        [UnitTest]
        [TestMethod]
        public void GetAllRequiredSourceDataCookersWithoutFinalizeThrows()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = new DataCookerPath(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestSourceDataCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));

            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.GetAllRequiredSourceDataCookers());
        }

        [UnitTest]
        [TestMethod]
        public void GetDataExtensionRetrievalWithoutFinalizeThrows()
        {
            // Just make sure that this doesn't throw an exception

            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor(TestTable1.SourceParserId);

            var sourceCooker = new TestSourceDataCooker() { Path = new DataCookerPath(TestTable1.SourceParserId, "CookerId1") };
            var sourceCookerReference = new TestSourceDataCookerReference(false)
            { availability = DataExtensionAvailability.Available, Path = sourceCooker.Path };

            cdp.ExtensionRepository.sourceCookersByPath.Add(sourceCooker.Path, sourceCookerReference);

            Assert.IsTrue(cdp.ExtensibilitySupport.AddTable(TestTable1.TableDescriptor));

            Assert.ThrowsException<InvalidOperationException>(() => cdp.ExtensibilitySupport.GetDataExtensionRetrieval(TestTable1.TableDescriptor));
        }

        [TestMethod]
        [UnitTest]
        public void QueryInvalidSourceParserIdThrows()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            Assert.ThrowsException<ArgumentException>(
                () => cdp.QueryOutput(new DataCookerPath("InvalidParserId", "RandomeCookerName"), "DataId"));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryInvalidSourceParserIdFails()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            var success = cdp.TryQueryOutput(
                new DataCookerPath("InvalidParserId", "RandomeCookerName"),
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
                () => cdp.QueryOutput<int>(new DataCookerPath("InvalidParserId", "RandomeCookerName"), "DataId"));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryInvalidSourceParserIdGenericFails()
        {
            var cdp = TestCustomDataProcessor.CreateTestCustomDataProcessor();
            var success = cdp.TryQueryOutput(
                new DataCookerPath("InvalidParserId", "RandomCookerName"),
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

            var dataCookerPath = new DataCookerPath(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestSourceDataCookerReference(false)
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

            var dataCookerPath = new DataCookerPath(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestSourceDataCookerReference(false)
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

            var dataCookerPath = new DataCookerPath(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestSourceDataCookerReference(false)
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

            var dataCookerPath = new DataCookerPath(sourceParserId, "CookerId1");
            var cookedDataReflector = new TestCookedDataReflector(dataCookerPath);

            var sourceCooker = new TestSourceDataCooker() { Path = dataCookerPath };
            sourceCooker.SetCookedDataReflector(cookedDataReflector);

            var sourceCookerReference = new TestSourceDataCookerReference(false)
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
