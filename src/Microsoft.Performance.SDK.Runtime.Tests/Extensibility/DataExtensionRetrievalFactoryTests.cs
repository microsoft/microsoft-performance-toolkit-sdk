// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DataExtensionRetrievalFactoryTests
    {
        internal static TestCookedDataRetrieval CreateSourceCookerData()
        {
            return new TestCookedDataRetrieval();
        }

        internal static ProcessingSystemCompositeCookers CreateCompositeCookerData(IDataExtensionRepository repo)
        {
            return new ProcessingSystemCompositeCookers(repo);
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullRepo_Throws()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            Assert.ThrowsException<ArgumentNullException>(() =>
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, null));
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullSourceCookerData_Throws()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            Assert.ThrowsException<ArgumentNullException>(() =>
                new DataExtensionRetrievalFactory(null, compositeCookerData, dataExtensionRepository));
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullCompositeCookerData_Throws()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();

            Assert.ThrowsException<ArgumentNullException>(() =>
                new DataExtensionRetrievalFactory(sourceCookerData, null, dataExtensionRepository));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForCompositeDataCookerSucceeds()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var sourceDataCookerReference1 = new TestSourceDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = DataCookerPath.ForSource("Source1", "SourceCooker1"),
            };

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = DataCookerPath.ForComposite("CompositeCooker1"),
            };
            dataCookerReference.requiredDataCookers.Add(sourceDataCookerReference1.Path);

            dataExtensionRepository.sourceCookersByPath.Add(sourceDataCookerReference1.Path, sourceDataCookerReference1);
            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            var dataRetrieval = dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerReference.Path);

            Assert.IsNotNull(dataRetrieval);

            // check that the cache is working as expected

            var dataRetrieval2 = dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerReference.Path);

            Assert.IsNotNull(dataRetrieval2);
            Assert.IsTrue(object.ReferenceEquals(dataRetrieval, dataRetrieval2));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForCompositeDataCooker_MissingCookerThrows()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var cookerPath = DataCookerPath.ForComposite("CompositeCooker1");

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForCompositeDataCooker_NotAvailableCookerThrows()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Error,
                Path = DataCookerPath.ForComposite("CompositeCooker1"),
            };

            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerReference.Path));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForTableSucceeds()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var sourceDataCookerReference1 = new TestSourceDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = DataCookerPath.ForSource("Source1", "SourceCooker1"),
            };

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = DataCookerPath.ForComposite("CompositeCooker1"),
            };
            dataCookerReference.requiredDataCookers.Add(sourceDataCookerReference1.Path);

            var tableReference = new TestTableExtensionReference(false)
            {
                availability = DataExtensionAvailability.Available,
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{F0F20004-E159-447B-B122-BD820C2A9908}"), "Test Table", "Test Table"),
            };

            dataExtensionRepository.sourceCookersByPath.Add(sourceDataCookerReference1.Path, sourceDataCookerReference1);
            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);
            dataExtensionRepository.tablesById.Add(tableReference.TableDescriptor.Guid, tableReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            var dataRetrieval = dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableReference.TableDescriptor.Guid);

            Assert.IsNotNull(dataRetrieval);

            // check that the cache is working as expected

            var dataRetrieval2 = dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableReference.TableDescriptor.Guid);

            Assert.IsNotNull(dataRetrieval2);
            Assert.IsTrue(object.ReferenceEquals(dataRetrieval, dataRetrieval2));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForTable_MissingCookerThrows()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var tableId = Guid.Parse("{6BB197C8-B3BE-4926-B23A-6F01451D80A3}");

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableId));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForTable_NotAvailableCookerThrows()
        {
            var dataExtensionRepository = new TestDataExtensionRepository();
            var sourceCookerData = CreateSourceCookerData();
            var compositeCookerData = CreateCompositeCookerData(dataExtensionRepository);

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Error,
                Path = DataCookerPath.ForComposite("CompositeCooker1"),
            };

            var tableReference = new TestTableExtensionReference(false)
            {
                availability = DataExtensionAvailability.IndirectError,
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{F0F20004-E159-447B-B122-BD820C2A9908}"), "Test Table", "Test Table"),
            };

            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);
            dataExtensionRepository.tablesById.Add(tableReference.TableDescriptor.Guid, tableReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(sourceCookerData, compositeCookerData, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableReference.TableDescriptor.Guid));
        }
    }
}
