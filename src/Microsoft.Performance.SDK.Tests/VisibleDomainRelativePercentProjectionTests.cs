// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class VisibleDomainRelativePercentProjectionTests
    {
        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnOriginalTest()
        {
            var projection = CreateProjection();

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);
            var baseline = projection[0];

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 2);
            var compare = projection[0];

            Assert.AreNotEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalDoesNotReceiveUpdateOnCloneTest()
        {
            var projection = CreateProjection();

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);
            var originalBaseline = projection[0];

            var clone = projection.CloneIfVisibleDomainSensitive();
            Assert.AreNotSame(projection, clone);

            //
            // We have to explicitly update the region on the original projection before
            // updating the clone because the original projection may have cached the value we
            // fetch above.
            //
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);

            //
            // Update the region on the clone. The original projection should receive the new
            // region too, giving us a different value when accessing it.
            //
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2);
            var originalCompare = projection[0];
            var cloneCompare = clone[0];

            Assert.AreEqual(originalBaseline, originalCompare);
            Assert.AreNotEqual(originalCompare, cloneCompare);
        }

        private static IProjection<int, double> CreateProjection()
        {
            return Projection.VisibleDomainRelativePercent.Create(Projection.Constant(1.0));
        }
    }
}
