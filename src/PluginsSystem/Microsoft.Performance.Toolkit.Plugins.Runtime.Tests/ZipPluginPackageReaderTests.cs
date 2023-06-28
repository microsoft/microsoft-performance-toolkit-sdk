﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class ZipPluginPackageReaderTests
    {
        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_InvalidZip_FailsWithErrorLogged()
        {
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            using var stream = new MemoryStream();
            var package = await sut.TryReadPackageAsync(stream, CancellationToken.None);

            Assert.IsNull(package);
            fakeLogger.Verify(logger => logger.Error(It.IsAny<InvalidDataException>(), It.IsAny<string>()));
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_StreamClosed_FailsWithErrorLogged()
        {
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            using var stream = new MemoryStream();
            stream.Close();
            var package = await sut.TryReadPackageAsync(stream, CancellationToken.None);

            Assert.IsNull(package);
            fakeLogger.Verify(logger => logger.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
        }

        [TestMethod]
        [UnitTest]
        public async Task TryReadPackageAsync_NoContentsFile_FailsWithErrorLogged()
        {
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginInfoFileName);
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
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginContentsInfoFileName);
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
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            fakeContentsInfoSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Throws<System.Text.Json.JsonException>();
            fakeInfoSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Throws<System.Text.Json.JsonException>();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginInfoFileName);
                    archive.CreateEntry(PackageConstants.PluginContentsInfoFileName);
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
            var fakeInfoSerializer = new Mock<ISerializer<PluginInfo>>();
            var fakeContentsInfoSerializer = new Mock<ISerializer<PluginContentsInfo>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var info = FakeInfo.GetFakePluginInfoWithOnlyIdentityAndSdkVersion();
            fakeInfoSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.FromResult(info));

            var contents = FakeContents.GetFakeEmptyPluginContentsInfo();
            fakeContentsInfoSerializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.FromResult(contents));

            var sut = new ZipPluginPackageReader(fakeInfoSerializer.Object, fakeContentsInfoSerializer.Object, fakeLoggerFactory);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry(PackageConstants.PluginInfoFileName);
                    archive.CreateEntry(PackageConstants.PluginContentsInfoFileName);
                    archive.CreateEntry(PackageConstants.PluginContentFolderName);
                }

                var package = await sut.TryReadPackageAsync(memoryStream, CancellationToken.None);

                Assert.IsNotNull(package);
                Assert.AreEqual(contents, package.PluginContentsInfo);
            }
        }
    }
}
