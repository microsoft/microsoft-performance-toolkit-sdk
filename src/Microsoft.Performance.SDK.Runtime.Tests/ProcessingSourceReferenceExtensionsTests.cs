// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataSourceAttribute = Microsoft.Performance.SDK.Processing.DataSourceAttribute;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class ProcessingSourceReferenceExtensionsTests
    {
        [TestMethod]
        [UnitTest]
        public void TryGetFileExtensions_NoFileDataSources_ReturnsEmpty()
        {
            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>());

            var actual = cdsr.TryGetFileExtensions();

            Assert.IsFalse(actual.Any());
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileExtensions_FileDataSources_ReturnsExtensions()
        {
            var extensions = new[]
            {
                new FileDataSourceAttribute(".txt"),
                new FileDataSourceAttribute("docx"),
                new FileDataSourceAttribute(".xlsx"),
            };

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>(extensions));

            var actual = cdsr.TryGetFileExtensions();

            CollectionAssert.AreEquivalent(
                extensions.Select(x => x.FileExtension).ToList(),
                actual.ToList());
        }

        [TestMethod]
        [UnitTest]
        public void TryGetCanonicalFileExtensions_NoFileDataSources_ReturnsEmpty()
        {
            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>());

            var actual = cdsr.TryGetCanonicalFileExtensions();

            Assert.IsFalse(actual.Any());
        }

        [TestMethod]
        [UnitTest]
        public void TryGetCanonicalFileExtensions_FileDataSources_ReturnsExtensions()
        {
            var extensions = new[]
            {
                new FileDataSourceAttribute(".txt"),
                new FileDataSourceAttribute("docx"),
                new FileDataSourceAttribute(".xlsx"),
            };

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>(extensions));

            var actual = cdsr.TryGetCanonicalFileExtensions();

            CollectionAssert.AreEquivalent(
                extensions.Select(x => FileExtensionUtils.CanonicalizeExtension(x.FileExtension)).ToList(),
                actual.ToList());
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileDescription_ExtensionlessNotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    fileAttr3,
                });

            var actual = cdsr.TryGetFileDescription(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileDescription_ExtensionlessFound_ReturnsDescription()
        {
            var testDescription = "Test Description";
            var extAttr = new ExtensionlessFileDataSourceAttribute(testDescription);

            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    extAttr,
                    fileAttr3,
                });

            var actual = cdsr.TryGetFileDescription(null);

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileDescription_ExtensionNotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    fileAttr3,
                });

            var actual = cdsr.TryGetFileDescription(".etl");

            Assert.IsNull(actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileDescription_ExtensionFound_ReturnsDescription()
        {
            var ext = ".docx";
            var testDescription = "Word";

            var fileAttr1 = new FileDataSourceAttribute(".txt", "Text");
            var fileAttr2 = new FileDataSourceAttribute(ext, testDescription);
            var fileAttr3 = new FileDataSourceAttribute(".xlsx", "Excel");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    fileAttr3,
                });

            var actual = cdsr.TryGetFileDescription(ext);

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetDirectoryDescription_NotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    fileAttr3,
                });

            var actual = cdsr.TryGetDirectoryDescription();

            Assert.IsNull(actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetDirectoryDescription_Found_ReturnsDescription()
        {
            var testDescription = "Test Description";
            var directoryAttr = new DirectoryDataSourceAttribute(testDescription);

            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    directoryAttr,
                    fileAttr3,
                });

            var actual = cdsr.TryGetDirectoryDescription();

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetExtensionlessFileDescription_NotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    fileAttr3,
                });

            var actual = cdsr.TryGetDirectoryDescription();

            Assert.IsNull(actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetExtensionlessFileDescription_Found_ReturnsDescription()
        {
            var testDescription = "Test Description";
            var directoryAttr = new ExtensionlessFileDataSourceAttribute(testDescription);

            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    fileAttr1,
                    fileAttr2,
                    directoryAttr,
                    fileAttr3,
                });

            var actual = cdsr.TryGetExtensionlessFileDescription();

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void AreDirectoriesSupportedTests()
        {
            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>());

            Assert.IsFalse(cdsr.AreDirectoriesSupported());

            cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new DirectoryDataSourceAttribute("Empty"),
                });

            Assert.IsTrue(cdsr.AreDirectoriesSupported());
        }

        [TestMethod]
        [UnitTest]
        public void AreExtensionlessFilesSupportedTests()
        {
            var cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>());

            Assert.IsFalse(cdsr.AreExtensionlessFilesSupported());

            cdsr = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new ExtensionlessFileDataSourceAttribute("Empty"),
                });

            Assert.IsTrue(cdsr.AreExtensionlessFilesSupported());
        }
    }
}
