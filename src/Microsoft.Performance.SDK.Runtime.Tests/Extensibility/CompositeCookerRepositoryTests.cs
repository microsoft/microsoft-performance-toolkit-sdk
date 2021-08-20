// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class CompositeCookerRepositoryTests
    {
        private TestDataExtensionRepository dataExtensionRepository;
        private ProcessingSystemCompositeCookers Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.dataExtensionRepository = new TestDataExtensionRepository();
            this.Sut = new ProcessingSystemCompositeCookers(this.dataExtensionRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.Sut.Dispose();
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullParam_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ProcessingSystemCompositeCookers(null));
        }

        [TestMethod]
        [UnitTest]
        public void Initialize_NullParam_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.Initialize(null));
        }

        [TestMethod]
        [UnitTest]
        public void GetCookerOutput_Success()
        {
            var sourceCookerData = DataExtensionRetrievalFactoryTests.CreateSourceCookerData();

            var sourceDataCookerReference1 = new TestSourceDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = DataCookerPath.ForSource("Source1", "SourceCooker1"),
            };

            var dataCookerReference = new TestCompositeDataCookerReference(false)
            {
                availability = DataExtensionAvailability.Available,
                Path = TestCompositeDataCooker.CookerPath,
            };

            var testCompositeCooker = new TestCompositeDataCooker();

            dataCookerReference.requiredDataCookers.Add(sourceDataCookerReference1.Path);

            dataExtensionRepository.sourceCookersByPath.Add(sourceDataCookerReference1.Path, sourceDataCookerReference1);
            dataExtensionRepository.compositeCookersByPath.Add(dataCookerReference.Path, dataCookerReference);

            dataCookerReference.createInstance = () => { return testCompositeCooker; };

            var retrievalFactory = new DataExtensionRetrievalFactory(
                sourceCookerData,
                this.Sut,
                this.dataExtensionRepository);

            this.Sut.Initialize(retrievalFactory);

            var cookerOutput = this.Sut.GetCookerOutput(dataCookerReference.Path);
            Assert.AreEqual((ICookedDataRetrieval)testCompositeCooker, cookerOutput);

            // if this is called a second time, it will throw an exception
            // the cooker should be cached, and this shouldn't be hit
            dataCookerReference.createInstance = () => { throw new InvalidOperationException(); };

            cookerOutput = this.Sut.GetCookerOutput(dataCookerReference.Path);
            Assert.AreEqual((ICookedDataRetrieval)testCompositeCooker, cookerOutput);
        }
    }
}
