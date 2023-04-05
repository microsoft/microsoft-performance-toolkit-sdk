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
    public class ZipPluginPackageReaderTests
    {
        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_InvalidZip_FailsWithErrorLogged()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            using var stream = new MemoryStream();
            var package = await sut.TryReadPackageAsync(stream, CancellationToken.None);

            Assert.IsNull(package);
            fakeLogger.Verify(logger => logger.Error(It.IsAny<InvalidDataException>(), It.IsAny<string>()));
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_StreamClosed_FailsWithErrorLogged()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            using var stream = new MemoryStream();
            stream.Close();
            var package = await sut.TryReadPackageAsync(stream, CancellationToken.None);

            Assert.IsNull(package);
            fakeLogger.Verify(logger => logger.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_NoMetadata_FailsWithErrorLogged()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginContentFolderName);
                }

                var package = await sut.TryReadPackageAsync(memoryStream, CancellationToken.None);

                Assert.IsNull(package);
                fakeLogger.Verify(logger => logger.Error(It.IsAny<string>()));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_NoContentFolder_FailsWithErrorLogged()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginMetadataFileName);
                }

                var package = await sut.TryReadPackageAsync(memoryStream, CancellationToken.None);

                Assert.IsNull(package);
                fakeLogger.Verify(logger => logger.Error(It.IsAny<string>()));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_DeserializationThrows_FailedWithErrorLogged()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            fakeSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Throws<System.Text.Json.JsonException>();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginMetadataFileName);
                    archive.CreateEntry(PackageConstants.PluginContentFolderName);
                }

                var package = await sut.TryReadPackageAsync(memoryStream, CancellationToken.None);

                Assert.IsNull(package);
                fakeLogger.Verify(logger => logger.Error(It.IsAny<System.Text.Json.JsonException>(), It.IsAny<string>()));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task PluginPackage_ValidPackage_Succeeds()
        {
            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var metadata = FakeMetadata.GetFakePluginMetadataWithOnlyIdentity();
            fakeSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.FromResult(metadata));

            var sut = new ZipPluginPackageReader(fakeSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginMetadataFileName);
                    archive.CreateEntry(PackageConstants.PluginContentFolderName);
                }

                var package = await sut.TryReadPackageAsync(memoryStream, CancellationToken.None);

                Assert.IsNotNull(package);
                Assert.AreEqual(metadata, package.PluginMetadata);
            }
        }
    }
}
