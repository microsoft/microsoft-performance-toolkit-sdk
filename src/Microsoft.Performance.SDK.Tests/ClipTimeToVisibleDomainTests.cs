// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ClipTimeToVisibleDomain
    {
        private const int visibleDomainStartTimeSeconds = 5;
        private const int visibleDomainEndTimeSeconds = 10;

        private const double halfVisibleDomainSeconds = (visibleDomainEndTimeSeconds - visibleDomainStartTimeSeconds) / 2.0;
        private const double quarterVisibleDomainSeconds = halfVisibleDomainSeconds / 2.0;
        private const double visibleDomainMidpointSeconds = visibleDomainStartTimeSeconds + halfVisibleDomainSeconds;

        // [====================]
        private static TimeRange entireVisibleDomain = new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds));

        // [==========----------]
        private static TimeRange firstHalfVisibleDomain = new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds), Timestamp.FromSeconds(visibleDomainMidpointSeconds));

        // [=====---------------]
        private static TimeRange firstQuarterVisibleDomain = new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds), Timestamp.FromSeconds(visibleDomainStartTimeSeconds + quarterVisibleDomainSeconds));

        // [-----==========-----]
        private static TimeRange middleHalfVisibleDomain = new TimeRange(Timestamp.FromSeconds(visibleDomainMidpointSeconds - quarterVisibleDomainSeconds), Timestamp.FromSeconds(visibleDomainMidpointSeconds + quarterVisibleDomainSeconds));

        // [--------------------]
        private static TimeRange noPartVisibleDomain = new TimeRange(Timestamp.FromSeconds(visibleDomainEndTimeSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds + 1));

        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnOriginalTest_Create()
        {
            var projection = CreateProjection(Projection.Constant(middleHalfVisibleDomain));

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);
            var baseline = projection[0];

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 2, firstHalfVisibleDomain);
            var compare = projection[0];

            Assert.AreNotEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void CloneHasVisibleDomainOnCreation_Create()
        {
            var projection = CreateProjection(Projection.Constant(firstQuarterVisibleDomain));

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, firstHalfVisibleDomain);
            var baseline = projection[0];

            var clone = CloneProjection(projection);
            var compare = clone[0];

            Assert.AreEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalDoesNotReceiveUpdateOnCloneTest_Create()
        {
            var projection = CreateProjection(Projection.Constant(middleHalfVisibleDomain));

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);
            var originalBaseline = projection[0];

            var clone = CloneProjection(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2, firstHalfVisibleDomain);
            var originalCompare = projection[0];
            var cloneCompare = clone[0];

            Assert.AreEqual(originalBaseline, originalCompare);
            Assert.AreNotEqual(originalCompare, cloneCompare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalReceivesUpdateOnOriginalTest_CreatePercent()
        {
            var projection = CreatePercentProjection(Projection.Constant(firstQuarterVisibleDomain));
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);

            var baseline = projection[0];

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 2, noPartVisibleDomain);
            var compare = projection[0];

            Assert.AreEqual(25.0, baseline);
            Assert.AreEqual(0.0, compare);
        }

        [TestMethod]
        [UnitTest]
        public void CloneHasVisibleDomainOnCreation_CreatePercent()
        {
            var projection = CreatePercentProjection(Projection.Constant(firstQuarterVisibleDomain));
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);

            var baseline = projection[0];
            Assert.AreEqual(25.0, baseline);

            var clone = CloneProjection(projection);
            var compare = clone[0];

            Assert.AreEqual(baseline, compare);
        }

        [TestMethod]
        [UnitTest]
        public void OriginalDoesNotReceiveUpdateOnCloneTest_CreatePercent()
        {
            var projection = CreatePercentProjection(Projection.Constant(middleHalfVisibleDomain));
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);

            var originalBaseline = projection[0];
            Assert.AreEqual(50.0, originalBaseline);

            var clone = CloneProjection(projection);

            TestVisibleDomainRegion.UpdateVisibleDomainRegion(clone, 2, noPartVisibleDomain);
            var originalCompare = projection[0];
            var cloneCompare = clone[0];

            Assert.AreEqual(originalBaseline, originalCompare);
            Assert.AreNotEqual(originalCompare, cloneCompare);
        }

        [TestMethod]
        [UnitTest]
        public void CreateHasCorrectDataTest()
        {
            var input = new List<TimeRange>()
            {
                new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(visibleDomainEndTimeSeconds * 2)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainMidpointSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds + quarterVisibleDomainSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds - quarterVisibleDomainSeconds)),
            };

            var expected = new List<TimeRange>()
            {
                new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainMidpointSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds + quarterVisibleDomainSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds - quarterVisibleDomainSeconds)),
            };

            var projection = CreateProjection(Projection.Index(input));
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], projection[i]);
            }
        }

        [TestMethod]
        [UnitTest]
        public void CreatePercentHasCorrectDataTest()
        {
            var input = new List<TimeRange>()
            {
                new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(0), Timestamp.FromSeconds(visibleDomainEndTimeSeconds * 2)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainMidpointSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds)),
                new TimeRange(Timestamp.FromSeconds(visibleDomainStartTimeSeconds + quarterVisibleDomainSeconds), Timestamp.FromSeconds(visibleDomainEndTimeSeconds - quarterVisibleDomainSeconds)),
            };

            var expected = new List<double>()
            {
                100.0,
                100.0,
                50.0,
                50.0,
            };

            var projection = CreatePercentProjection(Projection.Index(input));
            TestVisibleDomainRegion.UpdateVisibleDomainRegion(projection, 1, entireVisibleDomain);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(Math.Abs(expected[i] - projection[i]) < double.Epsilon);
            }
        }

        private static IProjection<int, TimeRange> CreateProjection(IProjection<int, TimeRange> input)
        {
            return Projection.ClipTimeToVisibleDomain.Create(input);
        }

        private static IProjection<int, double> CreatePercentProjection(IProjection<int, TimeRange> input)
        {
            return Projection.ClipTimeToVisibleDomain.CreatePercentDouble(input);
        }

        private static IProjection<int, T> CloneProjection<T>(IProjection<int, T> projection)
        {
            var clone = projection.CloneIfVisibleDomainSensitive();
            Assert.AreNotSame(projection, clone);
            return clone;
        }
    }
}
