using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal interface IPluginManifestValidator
    {
        bool Validate(PluginManifest pluginManifest);
    }
}
