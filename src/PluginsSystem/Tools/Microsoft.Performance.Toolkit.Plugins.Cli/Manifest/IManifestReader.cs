namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public interface IManifestReader
    {
        PluginManifest? TryReadFromFile(string manifestFilePath);

        PluginManifest? TryReadInteractively();
    }
}
