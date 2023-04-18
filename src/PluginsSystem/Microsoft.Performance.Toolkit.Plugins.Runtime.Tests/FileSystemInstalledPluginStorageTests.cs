// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using AutoFixture;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
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

        #region AddAsync

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_ContentFile_AddedToCorretPath()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            using var stream = new MemoryStream();
            string fileName = "bar.txt";

            fakePluginPackageEntry.Setup(x => x.Open()).Returns(stream);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns(fileName);
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns("");

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);
            
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

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            string folderName = "foo/bar/";
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns(folderName);
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns(folderName);

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);

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

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            using var stream = new MemoryStream();

            fakePluginPackageEntry.Setup(x => x.Open()).Returns(stream);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(true);

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            string metadataDestPath = Path.Combine(this.tempDirectory, "metadata.txt");
            fakeDir.Setup(d => d.GetPluginMetadataFilePath(fakePluginPackage.Object.PluginIdentity)).Returns(metadataDestPath);

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

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();

            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);

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

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);

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

            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            var exception = new Exception("foo");
            fakePluginPackageEntry.Setup(x => x.Open()).Throws(exception);
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(true);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.ContentRelativePath).Returns("");
            fakePluginPackageEntry.SetupGet(x => x.RawPath).Returns("");

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(new[] { fakePluginPackageEntry.Object });

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginContentDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);

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

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_ChecksumReturnsForCorrectIntallRootDir()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            
            var fakePluginPackageEntry = new Mock<PluginPackageEntry>();
            fakePluginPackageEntry.SetupGet(x => x.IsPluginContentFile).Returns(false);
            fakePluginPackageEntry.SetupGet(x => x.IsMetadataFile).Returns(false);

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();
            var fakePluginPackage = new Mock<PluginPackage>(fakeMetadata, fakeLoggerFactory);
            fakePluginPackage.Setup(x => x.Entries).Returns(Array.Empty<PluginPackageEntry>());

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginRootDirectory(fakePluginPackage.Object.PluginIdentity)).Returns(this.tempDirectory);

            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();
            fakeChecksumCalculator.Setup(c => c.GetDirectoryChecksumAsync(this.tempDirectory)).ReturnsAsync("foo");

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            var checksum = await sut.AddAsync(fakePluginPackage.Object, CancellationToken.None, null);

            // Assert
            Assert.AreEqual(checksum, "foo");
        }

        #endregion

        #region RemoveAsync

        [TestMethod]
        [UnitTest]
        public async Task RemoveAsync_PluginInstallDirRemoved()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();
            var fakePluginIdentity = new PluginIdentity("foo", new Version(1, 0, 0));

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            fakeDir.Setup(d => d.GetPluginRootDirectory(fakePluginIdentity)).Returns(this.tempDirectory);

            string tempFile = Path.Combine(this.tempDirectory, Path.GetRandomFileName());
            using (FileStream _ = File.Create(tempFile))
            {
            }

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            await sut.RemoveAsync(fakePluginIdentity, CancellationToken.None);

            // Assert
            Assert.IsFalse(Directory.Exists(this.tempDirectory));
        }

        #endregion

        #region TryGetPluginMetadataAsync

        [TestMethod]
        [UnitTest]
        public async Task TryGetPluginMetadataAsync_PluginMetadataFileExists_PluginMetadataReturned()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            PluginMetadata fakeMetadata = new Fixture().Create<PluginMetadata>();

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakeMetadata);

            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();
            var fakePluginIdentity = new PluginIdentity("foo", new Version(1, 0, 0));

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            string metadataDestPath = Path.Combine(this.tempDirectory, "metadata.txt");
            fakeDir.Setup(d => d.GetPluginMetadataFilePath(fakePluginIdentity)).Returns(metadataDestPath);

            string tempFile = metadataDestPath;
            using (FileStream _ = File.Create(tempFile))
            {
            }

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            PluginMetadata result = await sut.TryGetPluginMetadataAsync(fakePluginIdentity, CancellationToken.None);

            // Assert
            Assert.AreEqual(result, fakeMetadata);
        }

        [TestMethod]
        [UnitTest]
        public async Task TryGetPluginMetadataAsync_PluginMetadataFileDoesNotExist_NullReturned()
        {
            // Arrange
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var fakeSerializer = new Mock<ISerializer<PluginMetadata>>();
            var fakeChecksumCalculator = new Mock<IDirectoryChecksumCalculator>();
            var fakePluginIdentity = new PluginIdentity("foo", new Version(1, 0, 0));

            var fakeDir = new Mock<IPluginsStorageDirectory>();
            string metadataDestPath = Path.Combine(this.tempDirectory, "metadata.txt");
            fakeDir.Setup(d => d.GetPluginMetadataFilePath(fakePluginIdentity)).Returns(metadataDestPath);

            var sut = new FileSystemInstalledPluginStorage(
                fakeDir.Object,
                fakeSerializer.Object,
                fakeChecksumCalculator.Object,
                fakeLoggerFactory);

            // Act
            PluginMetadata result = await sut.TryGetPluginMetadataAsync(fakePluginIdentity, CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }

        #endregion
    }
}
