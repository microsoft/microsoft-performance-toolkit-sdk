// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataSourceAttribute = Microsoft.Performance.SDK.Processing.DataSourceAttribute;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class CustomDataSourceReferenceExtensionsTests
    {
        [TestMethod]
        [UnitTest]
        public void TryGetFileExtensions_NoFileDataSources_ReturnsEmpty()
        {
            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new List<DataSourceAttribute>().AsReadOnly(),
            };

            var actual = fakeReference.TryGetFileExtensions();

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

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    extensions.Cast<DataSourceAttribute>().ToList()),
            };

            var actual = fakeReference.TryGetFileExtensions();

            CollectionAssert.AreEquivalent(
                extensions.Select(x => x.FileExtension).ToList(),
                actual.ToList());
        }

        [TestMethod]
        [UnitTest]
        public void TryGetCanonicalFileExtensions_NoFileDataSources_ReturnsEmpty()
        {
            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new List<DataSourceAttribute>().AsReadOnly(),
            };

            var actual = fakeReference.TryGetCanonicalFileExtensions();

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

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    extensions.Cast<DataSourceAttribute>().ToList()),
            };

            var actual = fakeReference.TryGetCanonicalFileExtensions();

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
            
            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetFileDescription(null);

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

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        extAttr,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetFileDescription(null);

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetFileDescription_ExtensionNotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetFileDescription(".etl");

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

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetFileDescription(ext);

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetDirectoryDescription_NotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetDirectoryDescription();

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


            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        directoryAttr,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetDirectoryDescription();

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void TryGetExtensionlessFileDescription_NotFound_ReturnsNull()
        {
            var fileAttr1 = new FileDataSourceAttribute(".txt");
            var fileAttr2 = new FileDataSourceAttribute(".docx");
            var fileAttr3 = new FileDataSourceAttribute(".xlsx");

            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetDirectoryDescription();

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


            var fakeReference = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource))
            {
                DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(
                    new List<DataSourceAttribute>
                    {
                        fileAttr1,
                        fileAttr2,
                        directoryAttr,
                        fileAttr3,
                    }),
            };

            var actual = fakeReference.TryGetExtensionlessFileDescription();

            Assert.AreEqual(testDescription, actual);
        }

        [TestMethod]
        [UnitTest]
        public void AreDirectoriesSupportedTests()
        {
            var fake = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource));

            fake.DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(new List<DataSourceAttribute>());

            Assert.IsFalse(fake.AreDirectoriesSupported());

            fake.DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(new List<DataSourceAttribute> { new DirectoryDataSourceAttribute(), });

            Assert.IsTrue(fake.AreDirectoriesSupported());
        }

        [TestMethod]
        [UnitTest]
        public void AreExtensionlessFilesSupportedTests()
        {
            var fake = new FakeCustomDataSourceReference(typeof(FakeCustomDataSource));

            fake.DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(new List<DataSourceAttribute>());

            Assert.IsFalse(fake.AreExtensionlessFilesSupported());

            fake.DataSourcesSetter = new ReadOnlyCollection<DataSourceAttribute>(new List<DataSourceAttribute> { new ExtensionlessFileDataSourceAttribute(), });

            Assert.IsTrue(fake.AreExtensionlessFilesSupported());
        }
    }
}
