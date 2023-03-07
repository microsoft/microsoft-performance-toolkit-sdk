
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    public class FakeDiscoverer_2
        : IPluginDiscoverer
    {
        private ILogger logger;
        
        public Task<IReadOnlyCollection<AvailablePluginInfo>> DiscoverAllVersionsOfPluginAsync(PluginIdentity pluginIdentity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, AvailablePluginInfo>> DiscoverPluginsLatestAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PluginMetadata> GetPluginMetadataAsync(PluginIdentity pluginIdentity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
