// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class FileDataSourceAttributeTests
    {
        [TestMethod]
        [UnitTest]
        public void NullExtensionThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new FileDataSourceAttribute(null));
        }

        [TestMethod]
        [UnitTest]
        public void WhitespaceExtensionThrows()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new FileDataSourceAttribute(string.Empty));
        }

        [TestMethod]
        [UnitTest]
        public void NullDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new FileDataSourceAttribute("ext", null));
        }

        [TestMethod]
        [UnitTest]
        public void WhitespaceDescriptionThrows()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new FileDataSourceAttribute("ext", string.Empty));
        }

        [TestMethod]
        [UnitTest]
        public void ExtensionIsSet()
        {
            var extension = "ext";

            var sut = new FileDataSourceAttribute(extension);

            Assert.AreEqual(extension, sut.FileExtension);
        }

        [TestMethod]
        [UnitTest]
        public void DescriptionIsSetToExtension()
        {
            var extension = "ext";

            var sut = new FileDataSourceAttribute(extension);

            Assert.AreEqual(extension, sut.Description);
        }

        [TestMethod]
        [UnitTest]
        public void DescriptionIsSetWhenProvided()
        {
            var extension = "ext";
            var description = "This is a test.";

            var sut = new FileDataSourceAttribute(extension, description);

            Assert.AreEqual(description, sut.Description);
        }

        [TestMethod]
        [UnitTest]
        public void LeadingDotsAreRemovedFromExtension()
        {
            var extension = "ext";

            var sut = new FileDataSourceAttribute(".." + "ext");

            Assert.AreEqual(extension, sut.FileExtension);
        }
    }
}
