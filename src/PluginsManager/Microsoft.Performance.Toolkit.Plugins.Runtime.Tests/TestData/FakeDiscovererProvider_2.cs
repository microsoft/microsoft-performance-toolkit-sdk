
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    [PluginsManagerResource("F35F39B2-D8A2-4CD6-AAF2-78A0F4A60E57")]
    public class FakeDiscovererProvider_2
        : IPluginDiscovererProvider
    {
        private ILogger logger;

        public IPluginDiscoverer CreateDiscoverer(PluginSource pluginSource)
        {
            return new FakeDiscoverer_2();
        }

        public Task<bool> IsSupportedAsync(PluginSource pluginSource)
        {
            return Task.FromResult(pluginSource == PluginSources.PluginSource1 || pluginSource == PluginSources.PluginSource2);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
