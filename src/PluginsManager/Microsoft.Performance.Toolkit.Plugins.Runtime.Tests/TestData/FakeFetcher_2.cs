using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    [PluginsManagerResource("392FDE81-0FCA-46DC-9FF2-E96889187A20")]
    public class FakeFetcher_2
        : IPluginFetcher
    {
        private ILogger logger;

        public Task<Stream> GetPluginStreamAsync(AvailablePluginInfo pluginInfo, CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.FromResult(Stream.Null);
        }

        public Task<bool> IsSupportedAsync(AvailablePluginInfo pluginInfo)
        {
            return Task.FromResult(false);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
