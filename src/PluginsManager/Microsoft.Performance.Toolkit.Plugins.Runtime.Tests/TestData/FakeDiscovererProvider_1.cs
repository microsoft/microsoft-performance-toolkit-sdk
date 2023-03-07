// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    [PluginsManagerResource("91556A18-E1B7-40A2-AD2D-7A9C98B152AA")]
    public class FakeDiscovererProvider_1
        : IPluginDiscovererProvider
    {
        private ILogger logger;

        public IPluginDiscoverer CreateDiscoverer(PluginSource pluginSource)
        {
            return new FakeDiscoverer_1();
        }

        public Task<bool> IsSupportedAsync(PluginSource pluginSource)
        {
            return Task.FromResult(pluginSource == PluginSources.PluginSource1);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
