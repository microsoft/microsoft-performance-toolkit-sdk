// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.Performance.SDK.Runtime.Progress;
using Microsoft.Performance.SDK.Runtime.Tests.Fixtures;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Progress;

[TestClass]
public class PercentageProgressAggregatorTests
{
    [TestInitialize]
    public void Init()
    {
        // Calls to Report run on background thread, so we need to override their captured context
        // to run everything synchronously.
        SynchronizationContext.SetSynchronizationContext(new TestSynchronizationContext());
    }

    [TestMethod]
    [UnitTest]
    public void CorrectPercentageAggregated()
    {
        var spy = new ProgressSpy();
        var sut = new PercentageProgressAggregator(spy);

        var child0 = sut.CreateChild(0);
        var child1 = sut.CreateChild(0);

        child0.Report(50);
        Assert.AreEqual(25, spy.CurrentValue);

        child1.Report(50);
        Assert.AreEqual(50, spy.CurrentValue);

        child1.Report(100);
        Assert.AreEqual(75, spy.CurrentValue);
    }

    [TestMethod]
    [UnitTest]
    public void AddChildUpdatesAggregatedValue()
    {
        var spy = new ProgressSpy();
        var sut = new PercentageProgressAggregator(spy);

        var child1 = sut.CreateChild(0);
        child1.Report(100);

        sut.CreateChild(0);
        Assert.AreEqual(50, spy.CurrentValue);
    }

    [TestMethod]
    [UnitTest]
    public void FinishUpdatesAggregatedValue()
    {
        var spy = new ProgressSpy();
        var sut = new PercentageProgressAggregator(spy);

        sut.CreateChild(0);
        sut.CreateChild(0);

        int finalValue = 100;
        sut.Finish(finalValue);

        Assert.AreEqual(finalValue, spy.CurrentValue);
    }

    private class ProgressSpy
        : IProgress<int>
    {
        public int CurrentValue { get; private set; }

        public void Report(int value)
        {
            this.CurrentValue = value;
        }
    }
}