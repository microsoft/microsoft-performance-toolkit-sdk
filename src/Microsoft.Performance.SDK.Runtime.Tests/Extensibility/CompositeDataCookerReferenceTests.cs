﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.SDK.Tests.TestClasses;
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
                // TODO: __SDK_DP__
                // Redesign Data Processor API
                // Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataProcessors);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.CreateInstance(new FakeRetrieval()));
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
        public void CreateInstance()
        {
            var result = CompositeDataCookerReference.TryCreateReference(
                typeof(TestCompositeDataCooker),
                out var sut);

            Assert.IsTrue(result);

            var requiredData = new FakeRetrieval();
            IDataCooker instance = sut.CreateInstance(requiredData);

            Assert.IsNotNull(instance);

            var testCompositeCooker = instance as TestCompositeDataCooker;
            Assert.IsNotNull(testCompositeCooker);

            Assert.AreEqual(requiredData, testCompositeCooker.RequiredData);
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
                throw new NotImplementedException();
            }

            public bool TryQueryOutput(DataOutputPath identifier, out object result)
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
        }
    }
}
