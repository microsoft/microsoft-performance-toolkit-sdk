using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    public class FakeDiscoverer_1
        : IPluginDiscoverer
    {
        private ILogger logger;

        public async Task<IReadOnlyCollection<AvailablePluginInfo>> DiscoverAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            if (pluginIdentity == AvailablePluginInfos.AvailablePluginInfo_Source1_A_v1.Identity)
            {
                return new List<AvailablePluginInfo>()
                {
                    AvailablePluginInfos.AvailablePluginInfo_Source1_A_v1,
                    AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2
                };
            }
            else if (pluginIdentity == AvailablePluginInfos.AvailablePluginInfo_Source1_B.Identity)
            {
                return new List<AvailablePluginInfo>()
                {
                    AvailablePluginInfos.AvailablePluginInfo_Source1_B
                };
            }

            return new List<AvailablePluginInfo>();
        }

        public async Task<IReadOnlyDictionary<string, AvailablePluginInfo>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken)
        {
            var fakeResults = new Dictionary<string, AvailablePluginInfo>();
            fakeResults.Add(
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2.Identity.Id,
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2);

            fakeResults.Add(
                AvailablePluginInfos.AvailablePluginInfo_Source1_B.Identity.Id,
                AvailablePluginInfos.AvailablePluginInfo_Source1_B);

            return fakeResults;
        }

        public async Task<PluginMetadata> GetPluginMetadataAsync(PluginIdentity pluginIdentity, CancellationToken cancellationToken)
        {
            return new PluginMetadata();
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
