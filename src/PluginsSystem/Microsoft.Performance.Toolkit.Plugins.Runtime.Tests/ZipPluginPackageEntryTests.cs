// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO.Compression;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class ZipPluginPackageEntryTests
    {
        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginMetadataFileName, true)]
        [DataRow(@$"somefolder/{PackageConstants.PluginMetadataFileName}", false)]
        [DataRow(@"foo.json", false)]
        public void IsMetadataFile_ReturnsExpectedResult(string entryPath, bool expected)
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(entryPath);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().IsMetadataFile, expected);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginContentFolderName, true)]
        [DataRow(@$"{PackageConstants.PluginContentFolderName}foo.txt", true)]
        [DataRow(@"somefolder/", false)]
        public void IsContentFile_ReturnsExpectedResult(string entryPath, bool expected)
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(entryPath);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().IsPluginContentFile, expected);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        [DataRow(@"plugins\", @"plugins/")]
        [DataRow(@"plugins/foo\", @"plugins/foo/")]
        [DataRow(@"plugins/foo.txt", @"plugins/foo.txt")]
        [DataRow(@"plugins\foo.txt", @"plugins/foo.txt")]
        public void RawPath_ReturnsExpectedResult(string entryPath, string expectedRawPath)
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(entryPath);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().RawPath, expectedRawPath);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginMetadataFileName, null)]
        [DataRow(PackageConstants.PluginContentFolderName, "")]
        [DataRow(@$"{PackageConstants.PluginContentFolderName}foo.txt", @"foo.txt")]
        public void ContentRelativePath_ReturnsExpectedResult(string entryPath, string expectedContentRelativePath)
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(entryPath);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().ContentRelativePath, expectedContentRelativePath);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        public void Open_ReturnsExpectedResult()
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var fileEntry = archive.CreateEntry("foo.txt");

                    using (var entryStream = fileEntry.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("bar");
                    }
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);

                    using var openedStream = sut.Entries.Single().Open();
                    using var expectedStream = archive.GetEntry("foo.txt")?.Open();

                    using (var streamReader1 = new StreamReader(openedStream))
                    using (var streamReader2 = new StreamReader(expectedStream))
                    {
                        Assert.AreEqual(streamReader1.ReadToEnd(), streamReader2.ReadToEnd());
                    }
                }
            }
        }
    }
}
