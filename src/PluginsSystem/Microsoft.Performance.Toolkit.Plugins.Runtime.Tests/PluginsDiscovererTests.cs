// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class PluginsDiscovererTests
    {
        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_PluginSourceNotAdded_ExceptionThrown()
        {
            var fakePluginSourceRepo = new Mock<IRepositoryRO<PluginSource>>();
            fakePluginSourceRepo.SetupGet(x => x.Items).Returns(new List<PluginSource>());

            var fakeFetcherRepo = new Mock<IRepositoryRO<IPluginFetcher>>();
            var fakeDiscovererRepo = new Mock<IRepositoryRO<IPluginDiscovererProvider>>();
            var fakeValidator = new Mock<IPluginValidator>();
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new PluginsDiscoveryOrchestrator(
                fakePluginSourceRepo.Object,
                fakeFetcherRepo.Object,
                fakeDiscovererRepo.Object,
                fakeValidator.Object,
                fakeLoggerFactory);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await sut.GetAvailablePluginsLatestFromSourceAsync(
                    new PluginSource(FakeUris.Uri1),
                    CancellationToken.None));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_NoDiscoverer_ReturnsEmptyErrorRaised()
        {
            var fakePluginSource = new PluginSource(FakeUris.Uri1);

            var fakePluginSourceRepo = new Mock<IRepositoryRO<PluginSource>>();
            fakePluginSourceRepo.SetupGet(x => x.Items).Returns(new List<PluginSource>() { fakePluginSource });

            var fakeFetcherRepo = new Mock<IRepositoryRO<IPluginFetcher>>();
            var fakeDiscovererRepo = new Mock<IRepositoryRO<IPluginDiscovererProvider>>();
            var fakeLogger = new Mock<ILogger>();
            var fakeValidator = new Mock<IPluginValidator>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;

            var sut = new PluginsDiscoveryOrchestrator(
                fakePluginSourceRepo.Object,
                fakeFetcherRepo.Object,
                fakeDiscovererRepo.Object,
                fakeValidator.Object,
                fakeLoggerFactory);

            PluginSourceErrorEventArgs ? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                fakePluginSource,
                CancellationToken.None);

            Assert.IsFalse(result.Any());
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(fakePluginSource));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_SYSTEM_PluginsSystemResourceNotFound));
        }

        [TestMethod]
        [UnitTest]
        public async Task GetAvailablePluginsLatestFromSourceAsync_DiscovererNotSupported_ReturnsEmptyErrorRaised()
        {
            var fakePluginSource = new PluginSource(FakeUris.Uri1);
            var fakeDiscovererProvider = new Mock<IPluginDiscovererProvider>();

            var fakePluginSourceRepo = new Mock<IRepositoryRO<PluginSource>>();
            fakePluginSourceRepo.SetupGet(x => x.Items).Returns(new List<PluginSource>() { fakePluginSource });

            var fakeFetcherRepo = new Mock<IRepositoryRO<IPluginFetcher>>();
            var fakeDiscovererRepo = new Mock<IRepositoryRO<IPluginDiscovererProvider>>();
            fakeDiscovererRepo
                .SetupGet(x => x.Items)
                .Returns(new List<IPluginDiscovererProvider>() { fakeDiscovererProvider.Object });

            var fakeDiscoverer = new Mock<IPluginDiscoverer>();
            fakeDiscovererProvider.Setup(x => x.IsSupportedAsync(fakePluginSource)).ReturnsAsync(false);
            var fakeValidator = new Mock<IPluginValidator>();
            var fakeLogger = new Mock<ILogger>();
            Func<Type, ILogger> fakeLoggerFactory = (Type t) => fakeLogger.Object;
            
            var sut = new PluginsDiscoveryOrchestrator(
                fakePluginSourceRepo.Object,
                fakeFetcherRepo.Object,
                fakeDiscovererRepo.Object,
                fakeValidator.Object,
                fakeLoggerFactory);

            PluginSourceErrorEventArgs? eventArgs = null;
            sut.PluginSourceErrorOccured += (s, e) =>
            {
                eventArgs = e;
            };

            IReadOnlyCollection<AvailablePlugin> result = await sut.GetAvailablePluginsLatestFromSourceAsync(
                fakePluginSource,
                CancellationToken.None);

            Assert.IsFalse(result.Any());
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs!.PluginSource.Equals(fakePluginSource));
            Assert.IsTrue(eventArgs!.ErrorInfo.Code.Equals(ErrorCodes.PLUGINS_SYSTEM_PluginsSystemResourceNotFound));
        }
    }
}
