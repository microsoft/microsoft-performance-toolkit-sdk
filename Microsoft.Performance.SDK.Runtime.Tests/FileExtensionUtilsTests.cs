// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class FileExtensionUtilsTests
    {
        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_Null_ReturnsNull()
        {
            var ext = FileExtensionUtils.CanonicalizeExtension(null);

            Assert.IsNull(ext);
        }

        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_Empty_ReturnsEmpty()
        {
            var ext = FileExtensionUtils.CanonicalizeExtension(string.Empty);

            Assert.AreEqual(string.Empty, ext);
        }

        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_Whitespace_ReturnsEmpty()
        {
            var ext = FileExtensionUtils.CanonicalizeExtension(" \t ");

            Assert.AreEqual(string.Empty, ext);
        }

        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_ExtensionWithDotAtStart_ReturnsUppercased()
        {
            var original = ".txt";
            var expected = original.ToUpperInvariant();

            var result = FileExtensionUtils.CanonicalizeExtension(original);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_ExtensionWithoutDotAtStart_ReturnsWithDotUppercased()
        {
            var original = "txt";
            var expected = $".{original}".ToUpperInvariant();

            var result = FileExtensionUtils.CanonicalizeExtension(original);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [UnitTest]
        public void CanonicalizeExtension_ExtensionWithDotNotAtStart_ReturnsWithDotUppercased()
        {
            var original = "file.txt";
            var expected = $".{original}".ToUpperInvariant();

            var result = FileExtensionUtils.CanonicalizeExtension(original);

            Assert.AreEqual(expected, result);
        }

        //
        // GetCanonicalExtension
        //

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_Null_ReturnsNull()
        {
            var ext = FileExtensionUtils.GetCanonicalExtension(null);

            Assert.IsNull(ext);
        }

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_Empty_ReturnsEmpty()
        {
            var ext = FileExtensionUtils.GetCanonicalExtension(string.Empty);

            Assert.AreEqual(string.Empty, ext);
        }

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_Whitespace_ReturnsEmpty()
        {
            var original = " ";
            var expected = Path.GetExtension(original);

            var ext = FileExtensionUtils.GetCanonicalExtension(original);

            Assert.AreEqual(expected, ext);
        }

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_ExtensionWithDot_ReturnsUppercased()
        {
            var original = ".txt";
            var expected = original.ToUpperInvariant();

            var result = FileExtensionUtils.GetCanonicalExtension(original);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_ExtensionWithoutDot_ReturnsEmpty()
        {
            var original = "txt";
            var expected = string.Empty;

            var result = FileExtensionUtils.GetCanonicalExtension(original);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [UnitTest]
        public void GetCanonicalExtension_FilenameWithExtension_ReturnsCanonicalExtension()
        {
            var original = "file.txt";
            var expected = ".txt".ToUpperInvariant();

            var result = FileExtensionUtils.GetCanonicalExtension(original);

            Assert.AreEqual(expected, result);
        }
    }
}
