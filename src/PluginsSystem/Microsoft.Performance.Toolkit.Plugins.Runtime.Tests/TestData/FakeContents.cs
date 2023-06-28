using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    internal static class FakeContents
    {
        public static PluginContents GetFakeEmptyPluginContents()
        {
            return new PluginContents(
                null,
                null,
                null);
        }
    }

    internal static class FakeInfo
    {
        public static PluginInfo GetFakePluginInfoWithOnlyIdentityAndSdkVersion()
        {
            return new PluginInfo(
                new PluginIdentity("fake_id", new Version("1.0.0")),
                0,
                null,
                null,
                new Version("1.0.0"),
                null);
        }
    }
}
