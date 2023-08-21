using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Interfaces
{
    public interface IManifestReader
    {
        PluginManifest? TryReadFromFile(string manifestFilePath);

        PluginManifest? TryReadInteractively();
    }
}
