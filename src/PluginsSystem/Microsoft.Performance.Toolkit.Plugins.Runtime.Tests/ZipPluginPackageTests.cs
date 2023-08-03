// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Package;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class ZipPluginPackageTests
    {
        [TestMethod]
        [UnitTest]
        public void Constructor_EntriesCreated()
        {
            var info = FakeMetadata.GetFakeMetadataWithOnlyIdentityAndSdkVersion();
            var contents = FakeContentsMetadata.GetFakeEmptyPluginContentsMetadata();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginContentsMetadataFileName);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(info, contents, archive, fakeLoggerFactory);

                    Assert.AreEqual(sut.Entries.Count, 1);
                }
            }
        }
    }
}
