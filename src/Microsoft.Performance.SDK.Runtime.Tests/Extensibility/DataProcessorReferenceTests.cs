// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DataProcessorReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverthingThrows()
        {
            IDataProcessorReference sut = null;
            try
            {
                var result = DataProcessorReference.TryCreateReference(
                    typeof(TestDataProcessor),
                    out sut);

                Assert.IsTrue(result);

                sut.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => sut.Availability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DependencyReferences);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DependencyState);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Description);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Id);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.InitialAvailability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Name);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataCookers);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataProcessors);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.GetOrCreateInstance(new FakeRetrieval()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.PerformAdditionalDataExtensionValidation(new FakeSupport(), new FakeReference()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.ProcessDependencies(new TestDataExtensionRepository()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Release());
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            IDataProcessorReference sut = null;
            try
            {
                var result = DataProcessorReference.TryCreateReference(
                    typeof(TestDataProcessor),
                    out sut);

                Assert.IsTrue(result);

                sut.Dispose();
                sut.Dispose();
                sut.Dispose();
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_InstanceDisposed()
        {
            IDataProcessorReference sut = null;
            try
            {
                var result = DataProcessorReference.TryCreateReference(
                    typeof(DisposableDataProcessor),
                    out sut);

                Assert.IsTrue(result);

                var instance = sut.GetOrCreateInstance(new FakeRetrieval()) as DisposableDataProcessor;
                Assert.IsNotNull(instance);
                Assert.AreEqual(0, instance.DisposeCalls);

                sut.Dispose();

                Assert.AreEqual(1, instance.DisposeCalls);
            }
            finally
            {
                sut?.Dispose();
            }
        }

        [TestMethod]
        [UnitTest]
        public void WhenReleased_InstanceReset()
        {
            var result = DataProcessorReference.TryCreateReference(
                typeof(DisposableDataProcessor),
                out var sut);

            Assert.IsTrue(result);

            var instance = sut.GetOrCreateInstance(new FakeRetrieval()) as DisposableDataProcessor;
            Assert.IsNotNull(instance);
            Assert.AreEqual(0, instance.DisposeCalls);

            sut.Release();

            Assert.AreEqual(1, instance.DisposeCalls);
            var newInstance = sut.GetOrCreateInstance(new FakeRetrieval());
            Assert.AreNotSame(instance, newInstance);
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
        }
        private sealed class FakeSupport
            : IDataExtensionDependencyStateSupport
        {
            public void AddError(ErrorInfo error)
            {
                throw new NotImplementedException();
            }

            public void UpdateAvailability(DataExtensionAvailability availability)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class FakeReference
            : IDataExtensionReference
        {
            public string Name => throw new NotImplementedException();

            public DataExtensionAvailability InitialAvailability => throw new NotImplementedException();

            public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => throw new NotImplementedException();

            public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => throw new NotImplementedException();

            public DataExtensionAvailability Availability => throw new NotImplementedException();

            public IDataExtensionDependencies DependencyReferences => throw new NotImplementedException();

            public IDataExtensionDependencyState DependencyState => throw new NotImplementedException();

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void PerformAdditionalDataExtensionValidation(IDataExtensionDependencyStateSupport dependencyStateSupport, IDataExtensionReference requiredDataExtension)
            {
                throw new NotImplementedException();
            }

            public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
            {
                throw new NotImplementedException();
            }

            public void Release()
            {
                throw new NotImplementedException();
            }
        }
    }
}
