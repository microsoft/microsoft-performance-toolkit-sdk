// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataCookers;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    internal class InternalSourceDataCooker
        : SourceDataCooker<TestDataElement, TestDataContext, int>
    {
        public InternalSourceDataCooker()
            : base(DataCookerPath.ForSource("SourceId", "CookerId"))
        {
        }

        public override string Description { get; } = "Test Source Data Cooker";

        public override ReadOnlyHashSet<int> DataKeys { get; } = new ReadOnlyHashSet<int>(new HashSet<int>());

        public override DataProcessingResult CookDataElement(
            TestDataElement data,
            TestDataContext context,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NoEmptyPublicConstructorSourceDataCooker
        : SourceDataCooker<TestDataElement, TestDataContext, int>
    {
        public NoEmptyPublicConstructorSourceDataCooker(string sourceId, string cookerId)
            : base(sourceId, cookerId)
        {
        }

        public override string Description { get; } = "Test Source Data Cooker";

        public override ReadOnlyHashSet<int> DataKeys { get; } = new ReadOnlyHashSet<int>(new HashSet<int>());

        public override DataProcessingResult CookDataElement(
            TestDataElement data,
            TestDataContext context,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NoPublicConstructorSourceDataCooker
        : SourceDataCooker<TestDataElement, TestDataContext, int>
    {
        internal NoPublicConstructorSourceDataCooker()
            : base("SourceId", "CookerId")
        {
        }

        public override string Description { get; } = "Test Source Data Cooker";

        public override ReadOnlyHashSet<int> DataKeys { get; } = new ReadOnlyHashSet<int>(new HashSet<int>());

        public override DataProcessingResult CookDataElement(
            TestDataElement data,
            TestDataContext context,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    [TestClass]
    public class SourceDataCookerReferenceTests
    {
        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceSucceeds()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(ValidSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsTrue(result);
            Assert.IsNotNull(sourceDataCookerReference);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateInternalReferenceSucceeds()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(InternalSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsTrue(result);
            Assert.IsNotNull(sourceDataCookerReference);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceFails1()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(NoEmptyPublicConstructorSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsFalse(result);
            Assert.IsNull(sourceDataCookerReference);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceFails2()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(NoPublicConstructorSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsFalse(result);
            Assert.IsNull(sourceDataCookerReference);
        }

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverythingThrows()
        {
            ISourceDataCookerReference sut = null;
            try
            {
                var result =
                    SourceDataCookerReference.TryCreateReference(
                        typeof(ValidSourceDataCooker),
                        out sut);

                Assert.IsTrue(result);

                sut.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => sut.Availability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.DependencyReferences);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Description);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.InitialAvailability);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Name);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.Path);

                Assert.ThrowsException<ObjectDisposedException>(() => sut.CreateInstance());
                Assert.ThrowsException<ObjectDisposedException>(() => sut.PerformAdditionalDataExtensionValidation(new FakeSupport(), new FakeReference()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.ProcessDependencies(new TestDataExtensionRepository()));
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataCookers);
                Assert.ThrowsException<ObjectDisposedException>(() => sut.RequiredDataProcessors);
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
            ISourceDataCookerReference sut = null;
            try
            {
                var result =
                    SourceDataCookerReference.TryCreateReference(
                        typeof(ValidSourceDataCooker),
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
        public void WhenDisposed_AllCreatedInstancesDisposed()
        {
            ISourceDataCookerReference sut = null;
            try
            {
                var result =
                    SourceDataCookerReference.TryCreateReference(
                        typeof(DisposableSourceDataCooker),
                        out sut);

                Assert.IsTrue(result);

                var instances = Enumerable.Range(0, 3)
                    .Select(_ => sut.CreateInstance())
                    .Cast<DisposableSourceDataCooker>()
                    .ToList();

                sut.Dispose();

                foreach (var instance in instances)
                {
                    Assert.AreEqual(1, instance.DisposeCalls);
                }
            }
            finally
            {
                sut?.Dispose();
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
