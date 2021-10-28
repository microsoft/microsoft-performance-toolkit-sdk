// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DataExtensionRepositoryTests
    {
        private DataExtensionRepository Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.Sut = new DataExtensionRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.Sut.Dispose();
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverythingThrows()
        {
            this.Sut.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.CompositeDataCookers);
            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.DataProcessors);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.SourceDataCookers);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TablesById);

            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.GetCompositeDataCookerReference(new DataCookerPath()));
            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.GetDataProcessorReference(new DataProcessorId()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.GetSourceDataCookerFactory(new DataCookerPath()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.GetSourceDataCookerReference(new DataCookerPath()));
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            this.Sut.Dispose();
            this.Sut.Dispose();
            this.Sut.Dispose();
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_AllReferencesDisposed()
        {
            var sources = new[]
            {
                new TestRuntimeSourceCookerReference(),
                new TestRuntimeSourceCookerReference(),
                new TestRuntimeSourceCookerReference(),
            };

            var composites = new[]
            {
                new TestRuntimeCompositeCookerReference(),
                new TestRuntimeCompositeCookerReference(),
                new TestRuntimeCompositeCookerReference(),
            };

            // TODO: __SDK_DP__
            // Redesign Data Processor API
            //// var processors = new[]
            //// {
            ////     new TestRuntimeDataProcessorReference(),
            ////     new TestRuntimeDataProcessorReference(),
            ////     new TestRuntimeDataProcessorReference(),
            //// };

            var tables = new[]
            {
                new TestRuntimeTableExtensionReference(),
                new TestRuntimeTableExtensionReference(),
                new TestRuntimeTableExtensionReference(),
            };

            sources.ForEach(x => this.Sut.AddSourceDataCookerReference(x));
            composites.ForEach(x => this.Sut.AddCompositeDataCookerReference(x));
            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////processors.ForEach(x => this.Sut.AddDataProcessorReference(x));
            tables.ForEach(x => this.Sut.AddTableExtensionReference(x));

            this.Sut.Dispose();

            foreach (var source in sources)
            {
                Assert.AreEqual(1, source.DisposeCalls);
            }

            foreach (var composite in composites)
            {
                Assert.AreEqual(1, composite.DisposeCalls);
            }

            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////foreach (var processor in processors)
            ////{
            ////    Assert.AreEqual(1, processor.DisposeCalls);
            ////}

            foreach (var table in tables)
            {
                Assert.AreEqual(1, table.DisposeCalls);
            }
        }

        private sealed class FakeRetrieval
            : IDataExtensionRetrieval
        {
            public object QueryDataProcessor(DataProcessorId dataProcessorId)
            {
                throw new NotImplementedException();
            }

            public T QueryOutput<T>(DataOutputPath identifier)
            {
                throw new NotImplementedException();
            }

            public object QueryOutput(DataOutputPath identifier)
            {
                throw new NotImplementedException();
            }

            public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
            {
                result = default;
                return false;
            }

            public bool TryQueryOutput(DataOutputPath identifier, out object result)
            {
                result = default;
                return false;
            }
        }
    }
}
