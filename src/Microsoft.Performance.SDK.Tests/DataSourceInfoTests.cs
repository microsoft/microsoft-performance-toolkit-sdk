// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class DataSourceInfoTests
    {
        [TestMethod]
        [UnitTest]
        public void NegativeStartThrows()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(-1, 0, DateTime.UnixEpoch));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(-1, 0, DateTime.UnixEpoch));
        }

        [TestMethod]
        [UnitTest]
        public void NegativeEndThrows()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(0, -1, DateTime.UnixEpoch));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(0, -1, DateTime.UnixEpoch));
        }

        [TestMethod]
        [UnitTest]
        public void StopLessThanStartThrows()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(1, 0, DateTime.UnixEpoch));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataSourceInfo(1, 0, DateTime.UnixEpoch));
        }

        [TestMethod]
        [UnitTest]
        public void StartAndStopAndClockPopulated()
        {
            var start = 2;
            var stop = 3;
            var clock = DateTime.UtcNow;

            var sut = new DataSourceInfo(start, stop, clock);

            Assert.AreEqual(start, sut.FirstEventTimestampNanoseconds);
            Assert.AreEqual(stop, sut.EndTimestampNanoseconds);
            Assert.AreEqual(clock, sut.FirstEventWallClockUtc);
        }

        [TestMethod]
        [UnitTest]
        public void StartAndStopAndClockAndWallClockPopulated()
        {
            var start = 2;
            var stop = 3;
            var clock = DateTime.UtcNow;

            var sut = new DataSourceInfo(start, stop, clock);

            Assert.AreEqual(start, sut.FirstEventTimestampNanoseconds);
            Assert.AreEqual(stop, sut.EndTimestampNanoseconds);
            Assert.AreEqual(clock, sut.FirstEventWallClockUtc);

            Assert.AreEqual(clock.Ticks - (start / 100), sut.StartWallClockUtc.Ticks);
            Assert.AreEqual(clock.Ticks + (stop / 100), sut.EndWallClockUtc.Ticks);
        }
    }
}
