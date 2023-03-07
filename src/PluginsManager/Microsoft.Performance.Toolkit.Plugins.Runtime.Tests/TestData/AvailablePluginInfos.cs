using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    internal static class AvailablePluginInfos
    {
        public static AvailablePluginInfo AvailablePluginInfo_Source1_A_v1 = new AvailablePluginInfo(
            new PluginIdentity("Plugin_1_A", new Version("1.0.0")),
            PluginSources.PluginSource1,
            "Plugin_1_A firendly name",
            "Plugin_1_A description",
            new Uri(@"https://fetching.com/Plugin_1_A"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));

        public static AvailablePluginInfo AvailablePluginInfo_Source1_A_v2 = new AvailablePluginInfo(
            new PluginIdentity("Plugin_1_A", new Version("2.0.0")),
            PluginSources.PluginSource1,
            "Plugin_1_A firendly name",
            "Plugin_1_A description",
            new Uri(@"https://fetching.com/Plugin_1_A"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));

        public static AvailablePluginInfo AvailablePluginInfo_Source1_B = new AvailablePluginInfo(
            new PluginIdentity("Plugin_1_B", new Version("1.0.0")),
            PluginSources.PluginSource1,
            "Plugin_1_B firendly name",
            "Plugin_1_B description",
            new Uri(@"https://fetching.com/Plugin_1_B"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));
    };
}