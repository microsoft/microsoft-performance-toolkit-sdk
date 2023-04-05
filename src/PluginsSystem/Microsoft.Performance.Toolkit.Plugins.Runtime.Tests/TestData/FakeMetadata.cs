using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    internal static class FakeMetadata
    {
        public static PluginMetadata GetFakePluginMetadataWithOnlyIdentity()
        {
            return new PluginMetadata(
                "fake_id", new Version("1.0.0"), null, null, null, null, null, null, null);
        }
    }
}
