// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AutoFixture;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Fixture = AutoFixture.Fixture;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class FileSystemPluginRegistryTests
    {
        private string tempDirectory;

        public TestContext TestContext { get; set; }

        private string registryRoot;

        private string registryFilePath;

        [TestInitialize]
        public void Setup()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.tempDirectory);

            this.registryRoot = this.tempDirectory;
            this.registryFilePath = Path.GetFullPath(Path.Combine(this.registryRoot, FileBackedPluginRegistry.RegistryFileName));
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

        #region GetAllAsync

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_MissingData_ThrowsRepositoryCorruptedException()
        {
            File.Create(this.registryFilePath).Close();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Throws<ArgumentNullException>();

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<RepositoryCorruptedException>(() => sut.GetAllAsync(CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_FileNotExist_ReturnsEmpty()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(
                this.registryRoot,
                fakeSerializer.Object,
                Logger.Create);

            IReadOnlyCollection<InstalledPluginInfo> result = await sut.GetAllAsync(CancellationToken.None);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_FileOpenedByOtherProcess_ThrowsDataAccessException()
        {
            using (FileStream stream = File.Create(this.registryFilePath))
            {
                var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
                fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<InstalledPluginInfo>());

                var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

                await Assert.ThrowsExceptionAsync<RepositoryDataAccessException>(() => sut.GetAllAsync(CancellationToken.None));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_JsonException_ThrowsRepositoryCorruptedException()
        {
            File.Create(this.registryFilePath).Close();
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var exception = new JsonException();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var sut = new FileBackedPluginRegistry(
                this.registryRoot,
                fakeSerializer.Object,
                fakeLoggerFactory);

            RepositoryCorruptedException e = await Assert.ThrowsExceptionAsync<RepositoryCorruptedException>(() => sut.GetAllAsync(CancellationToken.None));
            Assert.AreSame(exception, e.InnerException);
            fakeLogger.Verify(l => l.Error(exception, It.IsAny<string>()));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_ReturnsRegisteredPluginsInfo()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var expectedResult = new List<InstalledPluginInfo>
            {
                fakeInstalledPluginInfo
            };

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>(expectedResult));

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            IReadOnlyCollection<InstalledPluginInfo> actualResult = await sut.GetAllAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expectedResult, actualResult.ToList());
        }

        #endregion


        #region TryGetByIdAsync

        [TestMethod]
        [UnitTest]
        public async Task TryGetByIdAsync_PluginExists_Returns()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>() { fakeInstalledPluginInfo });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            InstalledPluginInfo actualResult = await sut.TryGetByIdAsync(fakeInstalledPluginInfo.Metadata.Identity.Id, CancellationToken.None);

            Assert.AreEqual(fakeInstalledPluginInfo, actualResult);
        }

        [TestMethod]
        [UnitTest]
        public async Task TryGetByIdAsync_PluginDoesNotExist_ReturnsNull()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>());

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            InstalledPluginInfo actualResult = await sut.TryGetByIdAsync(fakeInstalledPluginInfo.Metadata.Identity.Id, CancellationToken.None);

            Assert.IsNull(actualResult);
        }

        [TestMethod]
        [UnitTest]
        public async Task TryGetByIdAsync_DuplicatesDetected_ThrowsRepositoryCorruptedException()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>() { fakeInstalledPluginInfo, fakeInstalledPluginInfo });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<RepositoryCorruptedException>(() => sut.TryGetByIdAsync(fakeInstalledPluginInfo.Metadata.Identity.Id, CancellationToken.None));
        }

        #endregion

        #region ExistsAsync

        [TestMethod]
        [UnitTest]
        public async Task ExistsAsync_Exists_ReturnsTrue()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>() { fakeInstalledPluginInfo });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            bool actualResult = await sut.ExistsAsync(fakeInstalledPluginInfo, CancellationToken.None);

            Assert.IsTrue(actualResult);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExistsAsync_DoesNotExist_ReturnsFalse()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>());

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            bool actualResult = await sut.ExistsAsync(fakeInstalledPluginInfo, CancellationToken.None);

            Assert.IsFalse(actualResult);
        }

        [TestMethod]
        [UnitTest]
        public async Task ExistsAsync_DuplicatesDetected_ThrowsRepositoryCorruptedException()
        {
            File.Create(this.registryFilePath).Close();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>() { fakeInstalledPluginInfo, fakeInstalledPluginInfo });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<RepositoryCorruptedException>(() => sut.ExistsAsync(fakeInstalledPluginInfo, CancellationToken.None));
        }

        #endregion

        #region AddAsync

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_FileNotExist_CreatesFilePluginAdded()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await sut.AddAsync(fakeInstalledPluginInfo, CancellationToken.None);

            Assert.IsTrue(File.Exists(this.registryFilePath));
            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Contains(fakeInstalledPluginInfo)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_PluginAdded()
        {
            File.Create(this.registryFilePath).Close();

            var fixture = new Fixture();
            InstalledPluginInfo plugin1 = fixture.Create<InstalledPluginInfo>();
            InstalledPluginInfo plugin2 = fixture.Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin1,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await sut.AddAsync(plugin2, CancellationToken.None);

            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Contains(plugin2) && x.Contains(plugin1)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_AlreadyRegisteredPlugin_ThrowsInvalidOperationException()
        {
            File.Create(this.registryFilePath).Close();

            var fixture = new Fixture();
            InstalledPluginInfo plugin1 = fixture.Create<InstalledPluginInfo>();

            var plugin2 = CreatePluginWithBumpedVersion(plugin1);

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin2
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.AddAsync(plugin1, CancellationToken.None));
        }

        #endregion

        #region DeleteAsync

        [TestMethod]
        [UnitTest]
        public async Task DeleteAsync_FileNotExist_ThrowsInvalidOperationException()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.DeleteAsync(fakeInstalledPluginInfo, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task DeleteAsync_PluginDeleted()
        {
            File.Create(this.registryFilePath).Close();

            var fixture = new Fixture();
            InstalledPluginInfo plugin1 = fixture.Create<InstalledPluginInfo>();
            InstalledPluginInfo plugin2 = fixture.Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin1,
                    plugin2,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await sut.DeleteAsync(plugin1, CancellationToken.None);

            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Single().Equals(plugin2)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [UnitTest]
        public async Task DeleteAsync_PluginNotRegistered_ThrowsInvalidOperationException()
        {
            File.Create(this.registryFilePath).Close();

            var fixture = new Fixture();
            InstalledPluginInfo plugin1 = fixture.Create<InstalledPluginInfo>();
            InstalledPluginInfo plugin2 = fixture.Create<InstalledPluginInfo>();

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin1,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.DeleteAsync(plugin2, CancellationToken.None));
        }


        [TestMethod]
        [UnitTest]
        public async Task DeleteAsync_PluginWithSameIdButDifferentVersionRegistered_ThrowsInvalidOperationException()
        {
            File.Create(this.registryFilePath).Close();

            var fixture = new Fixture();
            InstalledPluginInfo plugin1 = fixture.Create<InstalledPluginInfo>();
            var plugin2 = CreatePluginWithBumpedVersion(plugin1);

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin1,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.DeleteAsync(plugin2, CancellationToken.None));
        }

        #endregion

        #region UpdateAsync

        [TestMethod]
        [UnitTest]
        public async Task UpdateAsync_DifferentIDs_ThrowsInvalidOperationException()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();

            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();
            var fakeUpdatedPluginInfo = CreatePluginWithDifferentIdVersion(fakeInstalledPluginInfo);

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.UpdateAsync(fakeInstalledPluginInfo, fakeUpdatedPluginInfo, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task UpdateAsync_FileNotExist_ThrowsInvalidOperationException()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();

            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();
            var toUpdate = CreatePluginWithDifferentInstallDate(fakeInstalledPluginInfo);

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.UpdateAsync(fakeInstalledPluginInfo, toUpdate, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task UpdateAsync_CurrentPluginNotRegistered_ThrowsInvalidOperationException()
        {
            File.Create(this.registryFilePath).Close();

            InstalledPluginInfo current = new Fixture().Create<InstalledPluginInfo>();
            var toUpdate = CreatePluginWithDifferentInstallDate(current);

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>());

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.UpdateAsync(current, toUpdate, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task UpdateAsync_ToUpdatePluginAlreadyRegistered_ThrowsInvalidOperationException()
        {
            File.Create(this.registryFilePath).Close();

            InstalledPluginInfo current = new Fixture().Create<InstalledPluginInfo>();
            var toUpdate = CreatePluginWithDifferentInstallDate(current);

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    toUpdate,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.UpdateAsync(current, toUpdate, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task UpdateAsync_PluginUpdated()
        {
            File.Create(this.registryFilePath).Close();

            InstalledPluginInfo plugin1 = new Fixture().Create<InstalledPluginInfo>();
            var plugin2 = CreatePluginWithDifferentInstallDate(plugin1);

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin1,
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            await sut.UpdateAsync(plugin1, plugin2, CancellationToken.None);

            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Single().Equals(plugin2)),
                It.IsAny<CancellationToken>()));
        }

        #endregion

        #region AquireLock

        [TestMethod]
        [UnitTest]
        public async Task AquireLock_Succeeds()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            using (IDisposable _ = await sut.AquireLockAsync(CancellationToken.None, null))
            {
                Assert.IsTrue(File.Exists(Path.Combine(this.registryRoot, FileBackedPluginRegistry.LockFileName)));
            }

            Assert.IsFalse(File.Exists(Path.Combine(this.registryRoot, FileBackedPluginRegistry.LockFileName)));
        }

        [TestMethod]
        [UnitTest]
        public async Task AquireLock_Fails()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object, Logger.Create);

            Task<IDisposable> attempt;
            using (IDisposable _ = await sut.AquireLockAsync(CancellationToken.None, null))
            {
                attempt = sut.AquireLockAsync(CancellationToken.None, new TimeSpan(3000000));

                Assert.IsFalse(attempt.IsCompleted);
            }

            using (IDisposable _ = await attempt)
            {
                Assert.IsTrue(attempt.IsCompleted);
            }
        }

        #endregion

        private InstalledPluginInfo CreatePluginWithBumpedVersion(InstalledPluginInfo plugin)
        {
            return new InstalledPluginInfo(
                new PluginMetadata(
                    new PluginIdentity(
                        plugin.Metadata.Identity.Id,
                        new Version(plugin.Metadata.Identity.Version.Major + 1, 0, 0, 0)),
                    plugin.Metadata.InstalledSize,
                    plugin.Metadata.DisplayName,
                    plugin.Metadata.Description,
                    plugin.Metadata.SdkVersion,
                    plugin.Metadata.ProjectUrl,
                    plugin.Metadata.Owners),
                plugin.SourceUri,
                plugin.InstalledOn,
                plugin.Checksum);
        }

        private InstalledPluginInfo CreatePluginWithDifferentIdVersion(InstalledPluginInfo plugin)
        {
            return new InstalledPluginInfo(
                new PluginMetadata(
                    new PluginIdentity(
                        plugin.Metadata.Identity.Id + "1",
                        plugin.Metadata.Identity.Version),
                    plugin.Metadata.InstalledSize,
                    plugin.Metadata.DisplayName,
                    plugin.Metadata.Description,
                    plugin.Metadata.SdkVersion,
                    plugin.Metadata.ProjectUrl,
                    plugin.Metadata.Owners),
                plugin.SourceUri,
                plugin.InstalledOn,
                plugin.Checksum);
        }

        private InstalledPluginInfo CreatePluginWithDifferentInstallDate(InstalledPluginInfo plugin)
        {
            return new InstalledPluginInfo(
                new PluginMetadata(
                    new PluginIdentity(
                        plugin.Metadata.Identity.Id,
                        plugin.Metadata.Identity.Version),
                    plugin.Metadata.InstalledSize,
                    plugin.Metadata.DisplayName,
                    plugin.Metadata.Description,
                    plugin.Metadata.SdkVersion,
                    plugin.Metadata.ProjectUrl,
                    plugin.Metadata.Owners),
                plugin.SourceUri,
                DateTime.UtcNow,
                plugin.Checksum);
        }
    }
}
