// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataCookers;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    internal class InternalSourceDataCooker
        : BaseSourceDataCooker<TestDataElement, TestDataContext, int>
    {
        public InternalSourceDataCooker() 
            : base(new DataCookerPath("SourceId", "CookerId"))
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
        : BaseSourceDataCooker<TestDataElement, TestDataContext, int>
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
        : BaseSourceDataCooker<TestDataElement, TestDataContext, int>
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
        public void TryCreateReferenceFails1()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(InternalSourceDataCooker),
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
                    typeof(NoEmptyPublicConstructorSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsFalse(result);
            Assert.IsNull(sourceDataCookerReference);
        }

        [TestMethod]
        [UnitTest]
        public void TryCreateReferenceFails3()
        {
            var result =
                SourceDataCookerReference.TryCreateReference(
                    typeof(NoPublicConstructorSourceDataCooker),
                    out var sourceDataCookerReference);

            Assert.IsFalse(result);
            Assert.IsNull(sourceDataCookerReference);
        }
    }
}
