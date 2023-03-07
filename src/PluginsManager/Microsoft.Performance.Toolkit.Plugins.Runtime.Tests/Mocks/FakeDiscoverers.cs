using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Mocks
{
    public class FakeDiscover_1
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

            return new List<AvailablePluginInfo>();
        }

        public async Task<IReadOnlyDictionary<string, AvailablePluginInfo>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken)
        {
            var fakeResults = new Dictionary<string, AvailablePluginInfo>();
            fakeResults.Add(
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2.Identity.Id,
                AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2);

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
