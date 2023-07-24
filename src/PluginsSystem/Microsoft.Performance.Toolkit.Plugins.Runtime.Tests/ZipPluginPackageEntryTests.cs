// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Package;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class ZipPluginPackageEntryTests
    {
        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginContentsMetadataFileName, true)]
        [DataRow(@$"somefolder/{PackageConstants.PluginContentsMetadataFileName}", false)]
        [DataRow(@"foo.json", false)]
        public void ContentsFile_EntryType_ReturnsExpectedResult(string entryPath, bool expected)
        {
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
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
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().EntryType == PluginPackageEntryType.ContentsMetadataJsonFile, expected);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginContentFolderName, true)]
        [DataRow(@$"{PackageConstants.PluginContentFolderName}foo.txt", true)]
        [DataRow(@"somefolder/", false)]
        public void ContentFile_EntryType_ReturnsExpectedResult(string entryPath, bool expected)
        {
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
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
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().EntryType == PluginPackageEntryType.ContentFile, expected);
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
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
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
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().RawPath, expectedRawPath);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        [DataRow(PackageConstants.PluginContentsMetadataFileName, null)]
        [DataRow(PackageConstants.PluginContentFolderName, "")]
        [DataRow(@$"{PackageConstants.PluginContentFolderName}foo.txt", @"foo.txt")]
        public void ContentRelativePath_ReturnsExpectedResult(string entryPath, string expectedContentRelativePath)
        {
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
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
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Single().ContentRelativePath, expectedContentRelativePath);
                }
            }
        }

        [TestMethod]
        [UnitTest]
        public void Open_ReturnsExpectedResult()
        {
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
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
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

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
