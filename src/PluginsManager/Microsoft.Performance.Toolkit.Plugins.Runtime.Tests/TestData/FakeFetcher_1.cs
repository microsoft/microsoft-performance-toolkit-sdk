using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    [PluginsManagerResource("373E8E82-1AEE-4700-8D87-F05F2CFD7612")]
    public class FakeFetcher_1
        : IPluginFetcher
    {
        private ILogger logger;

        public Task<Stream> GetPluginStreamAsync(AvailablePluginInfo pluginInfo, CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.FromResult(Stream.Null);
        }

        public Task<bool> IsSupportedAsync(AvailablePluginInfo pluginInfo)
        {
            return Task.FromResult(pluginInfo.Equals(AvailablePluginInfos.AvailablePluginInfo_Source1_A_v2) ||
                pluginInfo.Equals(AvailablePluginInfos.AvailablePluginInfo_Source1_A_v1));
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
