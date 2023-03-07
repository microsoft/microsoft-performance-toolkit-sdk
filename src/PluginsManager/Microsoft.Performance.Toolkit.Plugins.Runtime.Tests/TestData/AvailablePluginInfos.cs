using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    internal static class AvailablePluginInfos
    {
        public static AvailablePluginInfo AvailablePluginInfo_Source1_A_v1 = new AvailablePluginInfo(
            new PluginIdentity("Plugin_A", new Version("1.0.0")),
            PluginSources.PluginSource1,
            "Plugin_A firendly name",
            "Plugin_A description",
            new Uri(@"https://fetching.com/Plugin_1_A"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));

        public static AvailablePluginInfo AvailablePluginInfo_Source1_A_v2 = new AvailablePluginInfo(
            new PluginIdentity("Plugin_A", new Version("2.0.0")),
            PluginSources.PluginSource1,
            "Plugin_A firendly name",
            "Plugin_A description",
            new Uri(@"https://fetching.com/Plugin_1_A"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));

        public static AvailablePluginInfo AvailablePluginInfo_Source1_B = new AvailablePluginInfo(
            new PluginIdentity("Plugin_B", new Version("1.0.0")),
            PluginSources.PluginSource1,
            "Plugin_B firendly name",
            "Plugin_B description",
            new Uri(@"https://fetching.com/Plugin_1_B"),
            new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));

        public static AvailablePluginInfo AvailablePluginInfo_Source2_A_v2 = new AvailablePluginInfo(
          new PluginIdentity("Plugin_A", new Version("2.0.0")),
          PluginSources.PluginSource2,
          "Plugin_A firendly name",
          "Plugin_A description",
          new Uri(@"https://fetching.com/Plugin_2_A"),
          new Guid("373E8E82-1AEE-4700-8D87-F05F2CFD7612"));
    };
}