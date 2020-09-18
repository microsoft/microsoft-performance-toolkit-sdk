// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class CollectionExtensionTests
    {
        [TestMethod]
        [UnitTest]
        public void AsReadOnlyArrayThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CollectionExtensions.AsReadOnly<object>((object[])null));
        }

        [TestMethod]
        [UnitTest]
        public void AsReadOnlyArrayReturnsReadOnlyWrapper()
        {
            var sut = new[] { 1, 2, 3, };

            var wrapped = sut.AsReadOnly();

            Assert.IsTrue(((ICollection<int>)wrapped).IsReadOnly);
            Assert.AreEqual(sut.Length, wrapped.Count);
            for (var i = 0; i < sut.Length; ++i)
            {
                Assert.AreEqual(sut[i], wrapped[i]);
            }
        }

        [TestMethod]
        [UnitTest]
        public void AsReadOnlyIListThrowsForNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CollectionExtensions.AsReadOnly<object>((IList<object>)null));
        }

        [TestMethod]
        [UnitTest]
        public void AsReadOnlyIListReturnsReadOnlyWrapper()
        {
            var sut = (IList<int>)(new[] { 1, 2, 3, });

            var wrapped = sut.AsReadOnly();

            Assert.IsTrue(((ICollection<int>)wrapped).IsReadOnly);
            Assert.AreEqual(sut.Count, wrapped.Count);
            for (var i = 0; i < sut.Count; ++i)
            {
                Assert.AreEqual(sut[i], wrapped[i]);
            }
        }
    }
}
