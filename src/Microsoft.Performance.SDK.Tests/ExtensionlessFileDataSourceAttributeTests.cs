// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ExtensionlessFileDataSourceAttributeTests
    {
        [TestMethod]
        [UnitTest]
        public void NullDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new ExtensionlessFileDataSourceAttribute(null));
        }

        [TestMethod]
        [UnitTest]
        public void WhitespaceDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new ExtensionlessFileDataSourceAttribute(string.Empty));
        }

        [TestMethod]
        [UnitTest]
        public void DescriptionIsSetWhenProvided()
        {
            var description = "This is a test.";

            var sut = new ExtensionlessFileDataSourceAttribute(description);

            Assert.AreEqual(description, sut.Description);
        }

        [TestMethod]
        [UnitTest]
        public void AcceptsFailsIfHasExtension()
        {
            var dataSource = new FileDataSource(@"c:\test\bad.ext");
            var sut = new ExtensionlessFileDataSourceAttribute("Empty");

            Assert.IsFalse(sut.Accepts(dataSource));
        }

        [TestMethod]
        [UnitTest]
        public void AcceptsSucceedsIfDoesNotHaveExtension()
        {
            var dataSource = new FileDataSource(@"c:\test\good");
            var sut = new ExtensionlessFileDataSourceAttribute("Empty");

            Assert.IsTrue(sut.Accepts(dataSource));
        }
    }
}
