// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class DirectoryDataSourceAttributeTests
    {
        [TestMethod]
        [UnitTest]
        public void NullDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new DirectoryDataSourceAttribute(null));
        }

        [TestMethod]
        [UnitTest]
        public void WhitespaceDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new DirectoryDataSourceAttribute(string.Empty));
        }

        [TestMethod]
        [UnitTest]
        public void DescriptionIsSetWhenProvided()
        {
            var description = "This is a test.";

            var sut = new DirectoryDataSourceAttribute(description);

            Assert.AreEqual(description, sut.Description);
        }
    }
}
