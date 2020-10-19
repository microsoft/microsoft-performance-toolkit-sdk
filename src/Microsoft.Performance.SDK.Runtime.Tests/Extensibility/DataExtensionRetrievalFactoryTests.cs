// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DataExtensionRetrievalFactoryTests
    {
        [TestMethod]
        [UnitTest]
        public void Constructor()
        {
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

            Assert.AreEqual(cookedDataRetrieval, dataExtensionRetrievalFactory.CookedSourceData);
            Assert.AreEqual(dataExtensionRepository, dataExtensionRetrievalFactory.DataExtensionRepository);
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForCompositeDataCookerSucceeds()
        {
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var sourceDataCookerReference1 = new TestSourceDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = new DataCookerPath("Source1", "SourceCooker1"),
            };

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = new DataCookerPath("CompositeCooker1"),
            };
            dataCookerReference.requiredDataCookers.Add(sourceDataCookerReference1.Path);

            dataExtensionRepository.sourceCookersByPath.Add(sourceDataCookerReference1.Path, sourceDataCookerReference1);
            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

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
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var cookerPath = new DataCookerPath("CompositeCooker1");

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForCompositeDataCooker_NotAvailableCookerThrows()
        {
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Error,
                Path = new DataCookerPath("CompositeCooker1"),
            };

            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerReference.Path));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForTableSucceeds()
        {
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var sourceDataCookerReference1 = new TestSourceDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = new DataCookerPath("Source1", "SourceCooker1"),
            };

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = new DataCookerPath("CompositeCooker1"),
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
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

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
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var tableId = Guid.Parse("{6BB197C8-B3BE-4926-B23A-6F01451D80A3}");

            var dataExtensionRetrievalFactory =
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableId));
        }

        [TestMethod]
        [UnitTest]
        public void CreateDataRetrievalForTable_NotAvailableCookerThrows()
        {
            var cookedDataRetrieval = new TestCookedDataRetrieval();
            var dataExtensionRepository = new TestDataExtensionRepository();

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Error,
                Path = new DataCookerPath("CompositeCooker1"),
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
                new DataExtensionRetrievalFactory(cookedDataRetrieval, dataExtensionRepository);

            Assert.ThrowsException<ArgumentException>(() =>
                dataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableReference.TableDescriptor.Guid));
        }
    }
}
