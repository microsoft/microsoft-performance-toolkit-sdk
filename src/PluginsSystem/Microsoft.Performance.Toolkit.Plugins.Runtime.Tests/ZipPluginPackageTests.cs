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
    public class ZipPluginPackageTests
    {
        [TestMethod]
        [UnitTest]
        public void Constructor_EntriesCreated()
        {
            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;
            
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginMetadataFileName);
                }
                
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    var sut = new ZipPluginPackage(metadata, archive, fakeLoggerFactory);
                    
                    Assert.AreEqual(sut.Entries.Count, 1);
                }
            }
        }
    }
}
