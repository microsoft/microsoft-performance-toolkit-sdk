// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Json;
using AutoFixture;
using Fixture = AutoFixture.Fixture;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    [DeploymentItem(@"TestFiles")]
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
        public async Task GetAllAsync_NoRegistryFile_ReturnsEmpty()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(
                this.registryRoot,
                fakeSerializer.Object);

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

                var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

                await Assert.ThrowsExceptionAsync<RepositoryDataAccessException>(() => sut.GetAllAsync(CancellationToken.None));
            }
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAllAsync_JsonException_ThrowsDataAccessException()
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

            RepositoryDataAccessException e = await Assert.ThrowsExceptionAsync<RepositoryDataAccessException>(() => sut.GetAllAsync(CancellationToken.None));
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

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            IReadOnlyCollection<InstalledPluginInfo> actualResult = await sut.GetAllAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expectedResult, actualResult.ToList()) ;
        }

        #endregion


        #region AddAsync

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_FileNotExist_CreatesFilePluginAdded()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            InstalledPluginInfo fakeInstalledPluginInfo = new Fixture().Create<InstalledPluginInfo>();

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            await sut.AddAsync(fakeInstalledPluginInfo, CancellationToken.None);

            Assert.IsTrue(File.Exists(this.registryFilePath));
            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Contains(fakeInstalledPluginInfo)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_FileExists_PluginAdded()
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

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            await sut.AddAsync(plugin2, CancellationToken.None);

            fakeSerializer.Verify(x => x.SerializeAsync(
                It.IsAny<Stream>(),
                It.Is<List<InstalledPluginInfo>>(x => x.Contains(plugin2) && x.Contains(plugin1)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_AlreadyRegisteredPlugin_ThrowsException()
        {
            File.Create(this.registryFilePath).Close();

            var plugin1 = new InstalledPluginInfo(
                new PluginIdentity("Test_id", new Version(1, 0, 0, 0)),
                new Uri("file://TestPlugin.dll"),
                "TestPlugin",
                "TestPlugin Description",
                DateTime.Now,
                "checksum");

            var plugin2 = new InstalledPluginInfo(
                new PluginIdentity("Test_id", new Version(2, 0, 0, 0)),
                new Uri("file://TestPlugin.dll"),
                "TestPlugin",
                "TestPlugin Description",
                DateTime.Now,
                "checksum");

            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            fakeSerializer.Setup(x => x.DeserializeAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()).Result)
                .Returns(new List<InstalledPluginInfo>
                {
                    plugin2
                });

            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => sut.AddAsync(plugin1, CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task AddAsync_FileOpenedByOtherProcess_ThrowsDataAccessException()
        {
            using Stream openedFileStream = File.Create(this.registryFilePath);
            var fixture = new Fixture();
            InstalledPluginInfo fakePluginInfo = fixture.Create<InstalledPluginInfo>();
            
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            await Assert.ThrowsExceptionAsync<RepositoryDataAccessException>(() => sut.AddAsync(fakePluginInfo, CancellationToken.None));
        }

        #endregion

        //#region DeleteAsync



        //#endregion

        #region AquireLock

        [TestMethod]
        [UnitTest]
        public async Task AquireLock_Success()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);
            
            using (IDisposable _ = await sut.AquireLockAsync(CancellationToken.None, null))
            {
                Assert.IsTrue(File.Exists(Path.Combine(this.registryRoot, FileBackedPluginRegistry.LockFileName)));
            }

            Assert.IsFalse(File.Exists(Path.Combine(this.registryRoot, FileBackedPluginRegistry.LockFileName)));
        }

        [TestMethod]
        [UnitTest]
        public async Task AquireLock_Fail()
        {
            var fakeSerializer = new Mock<ISerializer<List<InstalledPluginInfo>>>();
            var sut = new FileBackedPluginRegistry(this.registryRoot, fakeSerializer.Object);

            Task<IDisposable> attempt;
            using (IDisposable _ = await sut.AquireLockAsync(CancellationToken.None, null))
            {
                attempt = sut.AquireLockAsync(CancellationToken.None, null);

                await Task.Delay(3000);

                Assert.IsFalse(attempt.IsCompleted);
            }

            using (IDisposable _ = await attempt)
            {
                Assert.IsTrue(attempt.IsCompleted);
            }
        }


        #endregion

        //private InstalledPluginInfo CreateFakePluginInfo()
        //{
        //    return new InstalledPluginInfo(
        //        "TestPlugin1",
        //        new Version(1, 0, 0, 0),
        //        new Uri("file://TestPlugin.dll"),
        //        "TestPlugin",
        //        "TestPlugin Description",
        //        "c:\\TestPlugin",
        //        DateTime.Now,
        //        "checksum");
        //}

        //private InstalledPluginInfo CreateFakePluginInfo1()
        //{
        //    return new InstalledPluginInfo(
        //        "TestPlugin1",
        //        new Version(2, 0, 0, 0),
        //        new Uri("file://TestPlugin.dll"),
        //        "TestPlugin",
        //        "TestPlugin Description",
        //        "c:\\TestPlugin",
        //        DateTime.Now,
        //        "checksum");
        //}



        //private InstalledPluginInfo CreateFakePluginInfo2()
        //{
        //    return new InstalledPluginInfo(
        //        "TestPlugin2",
        //        new Version(1, 0, 0, 0),
        //        new Uri("file://TestPlugin.dll"),
        //        "TestPlugin",
        //        "TestPlugin Description",
        //        "c:\\TestPlugin",
        //        DateTime.Now,
        //        "checksum");
        //}
    }
}
