// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Manager;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Mocks;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ILogger = Microsoft.Performance.SDK.Processing.ILogger;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class PluginsManagerTests
    {
        private string TestInstallDir { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.TestInstallDir = Directory.CreateDirectory("TestPluginsManagerInstallation").FullName;
        }

        [TestMethod]
        public void Constructor_PluginSourcesInitialized()
        {
            PluginsManager sut = CreateSut();

            Assert.IsNotNull(sut.PluginSources);
        }

        #region ConfigurePluginSources

        [TestMethod]
        [UnitTest]
        public async Task AddPluginSources_SourcesAdded()
        {
            PluginsManager sut = CreateSut();
            PluginSource fakePluginSource = PluginSources.PluginSource1;

            await sut.AddPluginSourcesAsync(new PluginSource[] { PluginSources.PluginSource1 });

            Assert.ReferenceEquals(sut.PluginSources.Single(), fakePluginSource);
        }

        public async Task AddPluginSources_DiscovererProviderThrowsException_SourcesAddedErrorRaised()
        {
            var fakeDiscovererProvider = new Mock<IPluginDiscovererProvider>();
            fakeDiscovererProvider.Setup(x => x.IsSupportedAsync(It.IsAny <PluginSource>()))
                .ThrowsAsync(new Exception("Fake exception"));

            PluginsManager sut = CreateSut(
                new IPluginDiscovererProvider[] { fakeDiscovererProvider.Object });
            

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            await sut.AddPluginSourcesAsync(new PluginSource[] { PluginSources.PluginSource1 });

            Assert.ReferenceEquals(sut.PluginSources.Single(), PluginSources.PluginSource1);
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginSourceException));
        }

        [TestMethod]
        [UnitTest]
        public async Task AddPluginSources_DiscovererProviderThrowsException2_SourcesAddedErrorRaised()
        {
            Mock<IPluginDiscovererProvider> fakeDiscovererProvider = FakeDiscovererProvider.Create(true);
            fakeDiscovererProvider.Setup(x => x.CreateDiscoverer(It.IsAny<PluginSource>()))
                .Throws(new Exception("Fake exception"));

            PluginsManager sut = CreateSut(
                new IPluginDiscovererProvider[] { fakeDiscovererProvider.Object });


            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            await sut.AddPluginSourcesAsync(new PluginSource[] { PluginSources.PluginSource1 });

            Assert.ReferenceEquals(sut.PluginSources.Single(), PluginSources.PluginSource1);
            
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginSourceException));
        }


        [TestMethod]
        [UnitTest]
        public async Task ClearPluginSource_SourcesCleared()
        {
            PluginsManager sut = CreateSut();
            PluginSource fakePluginSource = PluginSources.PluginSource1;

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            await sut.AddPluginSourcesAsync(new PluginSource[] { fakePluginSource });
            sut.ClearPluginSources();

            Assert.IsFalse(sut.PluginSources.Any());
        }

        #endregion

        #region Discover

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_PluginSourceNotAdded_ExceptionThrown()
        {
            PluginsManager sut = CreateSut();

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await sut.GetAvailablePluginsLatestFromSourceAsync(
                    PluginSources.PluginSource1,
                    CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_NoDiscoverer_ReturnsEmptyErrorRaised()
        {
            PluginsManager sut = CreateSut();

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1 });

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource1,
                CancellationToken.None);

            Assert.IsFalse(result.Any());
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginsManagerResourceNotFound));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_DiscovererNotSupported_ReturnsEmptyErrorRaised()
        {
            PluginsManager sut = CreateSut(new IPluginDiscovererProvider[] {
                new FakeDiscovererProvider_1()
            });

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1,
                PluginSources.PluginSource2 });

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource2,
                CancellationToken.None);

            Assert.IsFalse(result.Any());
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource2));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginsManagerResourceNotFound));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_NoFetcher_ReturnsEmptyErrorRaised()
        {
            PluginsManager sut = CreateSut(new IPluginDiscovererProvider[] {
                new FakeDiscovererProvider_1()
            });

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1,
                PluginSources.PluginSource2 });

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource1,
                CancellationToken.None);

            Assert.IsFalse(result.Any());

            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginsManagerResourceNotFound));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_FetcherNotSupported_ReturnsSubsetErrorRaised()
        {
            PluginsManager sut = CreateSut(
                new IPluginDiscovererProvider[] { new FakeDiscovererProvider_1() },
                new IPluginFetcher[] { new FakeFetcher_1() });

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1 });

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource1,
                CancellationToken.None);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Single().AvailablePluginInfo.Equals(AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2));

            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginsManagerResourceNotFound));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_DiscovererThrowsException_ErrorRaised()
        {
            var fakeDiscoverer = new Mock<IPluginDiscoverer>();
            fakeDiscoverer.Setup(x => x.DiscoverPluginsLatestAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Fake exception"));

            PluginsManager sut = CreateSut(
                new IPluginDiscovererProvider[] { FakeDiscovererProvider.Create(fakeDiscoverer).Object },
                new IPluginFetcher[] { new FakeFetcher_1() });

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1 });

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource1,
                CancellationToken.None);

            Assert.IsFalse(result.Any());

            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(PluginSources.PluginSource1));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_MANAGER_PluginSourceException));
        }


        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_DuplicatesFromMultipleDiscoverers_ErrorRaised()
        {
            var fakeDiscoverer1 = new Mock<IPluginDiscoverer>();
            var fakeDiscoverer2 = new Mock<IPluginDiscoverer>();

            var fakeResults = new Dictionary<string, AvailablePluginInfo>();
            fakeResults.Add(AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2.Identity.Id,
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2);

            fakeDiscoverer1.Setup(x => x.DiscoverPluginsLatestAsync(It.IsAny<CancellationToken>()).Result)
                .Returns(fakeResults);
            fakeDiscoverer2.Setup(x => x.DiscoverPluginsLatestAsync(It.IsAny<CancellationToken>()).Result)
              .Returns(fakeResults);

            PluginsManager sut = CreateSut(
                new IPluginDiscovererProvider[] { 
                    FakeDiscovererProvider.Create(fakeDiscoverer1).Object,
                    FakeDiscovererProvider.Create(fakeDiscoverer2).Object,
                },
                new IPluginFetcher[] { new FakeFetcher_1() });

            await sut.AddPluginSourcesAsync(new PluginSource[] {
                PluginSources.PluginSource1 });

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                PluginSources.PluginSource1,
                CancellationToken.None);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(result.Single().AvailablePluginInfo, AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2);
        }

        #endregion

        private PluginsManager CreateSut(
        IEnumerable<IPluginDiscovererProvider>? providers = null,
        IEnumerable<IPluginFetcher>? fetchers = null)
        {
            var fakeRegistry = new Mock<IPluginRegistry>();
            var fakeDiscovererProvider = new Mock<IPluginDiscovererProvider>();
            var fakeFetcher = new Mock<IPluginFetcher>();
            var fakeLogger = new Mock<ILogger>();
            string installationDir = this.TestInstallDir;

            IEnumerable<IPluginDiscovererProvider> allProviders = providers == null ? new IPluginDiscovererProvider[] { fakeDiscovererProvider.Object } : providers;
            IEnumerable<IPluginFetcher> allFetchers = fetchers == null ? new IPluginFetcher[] { fakeFetcher.Object } : fetchers;

            return new PluginsManager(
                 allProviders,
                 allFetchers,
                 fakeRegistry.Object,
                 installationDir,
                 fakeLogger.Object);
        }
    }
}
