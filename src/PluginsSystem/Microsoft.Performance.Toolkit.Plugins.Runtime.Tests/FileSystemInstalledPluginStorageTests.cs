// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using AutoFixture;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Fixture = AutoFixture.Fixture;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class FileSystemInstalledPluginStorageTests
    {
        private string tempDirectory;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.tempDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                Directory.Delete(this.tempDirectory, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception e)
            {
                this.TestContext.WriteLine($"Failed to delete {this.tempDirectory}: {e}");
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_ContentFile_AddedToCorretPath()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(It.IsAny<PluginIdentity>())).Returns(this.tempDirectory);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            using var stream = new MemoryStream();
            string fileName = "bar.txt";

            fakePluginPackageEntry.Setup(x => x.Open()).Returns(stream);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns(fileName);
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns("");

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            string expectedPath = Path.Combine(this.tempDirectory, fileName);
            Assert.IsTrue(File.Exists(expectedPath));
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_ContentDirectory_CreatedAtCorrectPath()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(It.IsAny<PluginIdentity>())).Returns(this.tempDirectory);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            string folderName = "foo/bar/";
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns(folderName);
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns(folderName);

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            string expectedPath = Path.Combine(this.tempDirectory, folderName);
            Assert.IsTrue(Directory.Exists(expectedPath));
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_Metadata_AddedToCorrectPath()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            string metadataDestPath = Path.Combine(this.tempDirectory, "metadata.txt");
            fakeDir.Setup(d => d.GetPluginMetadataFilePath(It.IsAny<PluginIdentity>())).Returns(metadataDestPath);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            using var stream = new MemoryStream();

            fakePluginPackageEntry.Setup(x => x.Open()).Returns(stream);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(true);

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            Assert.IsTrue(File.Exists(metadataDestPath));
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_UnknownTypePackageEntry_NotAdded()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(It.IsAny<PluginIdentity>())).Returns(this.tempDirectory);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();

            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            Assert.IsFalse(Directory.EnumerateFileSystemEntries(this.tempDirectory).Any());
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_FileContentMatch()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(It.IsAny<PluginIdentity>())).Returns(this.tempDirectory);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            string fileName = "bar.txt";
            string fileContent = "foo";

            using var stream = new MemoryStream();
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 512, true))
            {
                streamWriter.Write(fileContent);
            }

            stream.Seek(0, SeekOrigin.Begin);
            fakePluginPackageEntry.Setup(x => x.Open()).Returns(stream);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns(fileName);
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns("");

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            string destPath = Path.Combine(this.tempDirectory, fileName);
            string? readContent;
            using (var sr = new StreamReader(destPath))
            {
                readContent = sr.ReadLine();
            }

            Assert.AreEqual(readContent, fileContent);
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_ExceptionThrownInExtractionLoop_ExceptionHandledAndErrorLogged()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(It.IsAny<PluginIdentity>())).Returns(this.tempDirectory);

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            var exception = new Exception("foo");
            fakePluginPackageEntry.Setup(x => x.Open()).Throws(exception);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns("");
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns("");

            PluginMetadata fixture = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fixture, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            PluginPackageExtractionException e = await Assert.ThrowsExceptionAsync<PluginPackageExtractionException>(
                async () => await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null));

            // Assert
            Assert.AreEqual(e.InnerException, exception);
            fakeLogger.Verify(l => l.Error(
                It.Is<Exception>(ex => ex.Message == "foo"),
                It.Is<string>(s => true)));
        }
    }
}
