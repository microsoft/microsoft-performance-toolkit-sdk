using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Installation
{
    public sealed class InstalledPlugin
    {
        public InstalledPlugin(
            InstalledPluginInfo installedPluginInfo,
            PluginMetadata pluginMetadata)
        {
            this.InstalledPluginInfo = installedPluginInfo;
            this.PluginMetadata = pluginMetadata;
        }

        public InstalledPluginInfo InstalledPluginInfo { get; }

        public PluginMetadata PluginMetadata { get; }
    }
}
