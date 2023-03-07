using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.TestData
{
    internal static class PluginSources
    {
        public static PluginSource PluginSource1 = new PluginSource(new Uri(@"http://fake.com/1"));
        public static PluginSource PluginSource2 = new PluginSource(new Uri(@"http://fake.com/2"));
        public static PluginSource PluginSource3 = new PluginSource(new Uri(@"http://fake.com/3"));
    }
}
