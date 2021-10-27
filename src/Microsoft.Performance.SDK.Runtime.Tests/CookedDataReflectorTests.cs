// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Tests.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class CookedDataReflectorTests
    {
        private TestCookedDataReflector testReflector = new TestCookedDataReflector();

        [TestMethod]
        [UnitTest]
        public void QueryHasDataAsObject()
        {
            var result = testReflector.QueryOutput(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.HasData)));

            Assert.AreEqual(testReflector.HasData, (bool)result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryHasDataAsBool()
        {
            var result = testReflector.QueryOutput<bool>(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.HasData)));

            Assert.AreEqual(testReflector.HasData, result);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryHasDataAsObject()
        {
            var success = testReflector.TryQueryOutput(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.HasData)),
                out var result);

            Assert.IsTrue(success);
            Assert.AreEqual(testReflector.HasData, (bool)result);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryHasDataAsBool()
        {
            var success = testReflector.TryQueryOutput<bool>(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.HasData)),
                out var result);

            Assert.IsTrue(success);
            Assert.AreEqual(testReflector.HasData, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryAddFuncAsObject()
        {
            var addFuncAsObject = testReflector.QueryOutput(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc)));

            int result = ((Func<int, int, int>)addFuncAsObject)(3, 4);
            Assert.AreEqual(7, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryAddFuncAsFunc()
        {
            var addFunc = testReflector.QueryOutput<Func<int, int, int>>(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc)));

            int result = addFunc(-10, 15);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryAddFuncAsObject()
        {
            var success = testReflector.TryQueryOutput(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc)),
                out var addFuncAsObject);

            Assert.IsTrue(success);

            int result = ((Func<int, int, int>)addFuncAsObject)(33, -33);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryAddFuncAsFunc()
        {
            var success = testReflector.TryQueryOutput<Func<int, int, int>>(
                new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc)),
                out var addFunc);

            Assert.IsTrue(success);

            int result = addFunc(-22, -13);
            Assert.AreEqual(-35, result);
        }

        [TestMethod]
        [UnitTest]
        public void QueryAsWrongTypeThrows()
        {
            Assert.ThrowsException<InvalidCastException>(
                () => testReflector.QueryOutput<bool>(
                    new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc))));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryAsWrongTypeFails()
        {
            var success = testReflector.TryQueryOutput(
                    new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, nameof(TestCookedDataReflector.AddFunc)),
                    out bool result);

            Assert.IsFalse(success);
            Assert.AreEqual(result, default);
        }

        [TestMethod]
        [UnitTest]
        public void QueryNonExistentPathThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => testReflector.QueryOutput<bool>(
                    new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, "DoesNotExist")));
        }

        [TestMethod]
        [UnitTest]
        public void TryQueryNonExistentPathFails()
        {
            var success = testReflector.TryQueryOutput(
                    new DataOutputPath(TestCookedDataReflector.DefaultCookerPath, "DoesNotExist"),
                    out Func<int> result);

            Assert.IsFalse(success);
            Assert.AreEqual(result, default);
        }
    }
}
