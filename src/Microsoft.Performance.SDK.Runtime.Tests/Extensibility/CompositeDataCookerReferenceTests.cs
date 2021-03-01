// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class CompositeDataCookerReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverthingThrows()
        {
            ICompositeDataCookerReference sut = null;
            try
            {
                var result = CompositeDataCookerReference.TryCreateReference(
                    typeof(TestCompositeDataCooker),
                    out sut);

                Assert.IsTrue(result);

                sut.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => sut.Availability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DependencyReferences);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Description);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.InitialAvailability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Name);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Path);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataCookers);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataProcessors);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.GetOrCreateInstance(new FakeRetrieval()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.PerformAdditionalDataExtensionValidation(new FakeSupport(), new FakeReference()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.ProcessDependencies(new TestDataExtensionRepository()));
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
            ICompositeDataCookerReference sut = null;
            try
            {
                var result = CompositeDataCookerReference.TryCreateReference(
                    typeof(TestCompositeDataCooker),
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
            ICompositeDataCookerReference sut = null;
            try
            {
                var result = CompositeDataCookerReference.TryCreateReference(
                    typeof(DisposableCompositeDataCooker),
                    out sut);

                Assert.IsTrue(result);

                var instance = sut.GetOrCreateInstance(new FakeRetrieval()) as DisposableCompositeDataCooker;
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
            public void AddError(string error)
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
        }
    }
}
