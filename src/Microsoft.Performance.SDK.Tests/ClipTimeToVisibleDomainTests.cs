// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ClipTimeToVisibleDomain
    {
        private const int projectionEndTimeSeconds = 5;

        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnOriginalTest_Create()
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
        public void OriginalReceivesUpdateOnCloneTest_Create()
        {
            var projection = CreateProjection();

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);
            var originalBaseline = projection[0];

            var clone = CloneProjection(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2);
            var originalCompare = projection[0];
            var cloneCompare = clone[0];

            Assert.AreNotEqual(originalBaseline, originalCompare);
            Assert.AreEqual(originalCompare, cloneCompare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnOriginalTest_CreatePercent()
        {
            var projection = CreatePercentProjection();

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);
            var baseline = projection[0];

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 2);
            var compare = projection[0];

            Assert.AreNotEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnCloneTest_CreatePercent()
        {
            var projection = CreatePercentProjection();

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1);
            var originalBaseline = projection[0];

            var clone = CloneProjection(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2);
            var originalCompare = projection[0];
            var cloneCompare = clone[0];

            Assert.AreNotEqual(originalBaseline, originalCompare);
            Assert.AreEqual(originalCompare, cloneCompare);
        }

        [TestMethod]
        [UnitTest]
        public void CreatePercentFormatterReceivesUpdateOnOriginalTest()
        {
            var projection = CreatePercentProjection();
            ICustomFormatter projectionFormatter = GetProjectionFormatter(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds)));
            var baseline = projectionFormatter.Format(null, projection[0], null);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 2, new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds * 2)));
            var compare = projectionFormatter.Format(null, projection[0], null);

            Assert.AreNotEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void CreatePercentFormatterReceivesUpdateOnCloneTest()
        {
            var projection = CreatePercentProjection();
            ICustomFormatter projectionFormatter = GetProjectionFormatter(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds)));
            var originalBaseline = projectionFormatter.Format(null, projection[0], null);

            var clone = CloneProjection(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2, new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds * 2)));
            var originalCompare = projectionFormatter.Format(null, projection[0], null);
            var cloneCompare = projectionFormatter.Format(null, clone[0], null);

            Assert.AreNotEqual(originalBaseline, originalCompare);
            Assert.AreEqual(originalCompare, cloneCompare);
        }

        private static IProjection<int, TimeRange> CreateProjection()
        {
            return Projection.ClipTimeToVisibleDomain.Create(Projection.Constant(new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds))));
        }

        private static IProjection<int, TimeRange> CreatePercentProjection()
        {
            return Projection.ClipTimeToVisibleDomain.CreatePercent(Projection.Constant(new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(projectionEndTimeSeconds))));
        }

        private static IProjection<int, TimeRange> CloneProjection(IProjection<int, TimeRange> projection)
        {
            var clone = projection.CloneIfVisibleDomainSensitive();
            Assert.AreNotSame(projection, clone);
            return clone;
        }

        private static ICustomFormatter GetProjectionFormatter(IProjection<int, TimeRange> projection)
        {
            Assert.IsTrue(projection is IFormatProvider);
            var projectionFormatter = (projection as IFormatProvider).GetFormat(typeof(TimeRange)) as ICustomFormatter;
            return projectionFormatter;
        }
    }
}
