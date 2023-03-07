using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Mocks
{
    public static class FakeDiscoverer
    {
        public static Mock<IPluginDiscoverer> CreateAlwaysEmpty()
        {
            var mockDiscoverer = new Mock<IPluginDiscoverer>();
            
            mockDiscoverer.Setup(x => x.DiscoverPluginsLatestAsync(
                CancellationToken.None).Result).Returns(new Dictionary<string, AvailablePluginInfo>());

            mockDiscoverer.Setup(x => x.DiscoverAllVersionsOfPluginAsync(
                It.IsAny<PluginIdentity>(), CancellationToken.None).Result).Returns(Array.Empty<AvailablePluginInfo>());

            return mockDiscoverer;
        }

        public static Mock<IPluginDiscoverer> Create(IEnumerable<AvailablePluginInfo> results)
        {
            results.ToDictionary(x => x.Identity.Id, x => x);

            var fakeResults = new Dictionary<string, AvailablePluginInfo>();
            fakeResults.Add(
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2.Identity.Id,
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2);

            var mockDiscoverer = new Mock<IPluginDiscoverer>();

            mockDiscoverer.Setup(x => x.DiscoverPluginsLatestAsync(
                CancellationToken.None).Result).Returns(fakeResults);

            mockDiscoverer.Setup(x => x.DiscoverAllVersionsOfPluginAsync(
                It.IsAny<PluginIdentity>(), CancellationToken.None).Result).Returns(Array.Empty<AvailablePluginInfo>());

            return mockDiscoverer;
        }
    }
}
