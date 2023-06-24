namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public interface IPluginManifestValidator
    {
        bool Validate(PluginManifest pluginManifest);
    }
}
