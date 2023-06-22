using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class PluginManifestValidator
        : IPluginManifestValidator
    {
        public bool Validate(string pluginManifestPath)
        {
            return true;
        }
    }
}
