using System.ComponentModel.DataAnnotations;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal class PluginIdentityManifest
    {
        public PluginIdentityManifest(
            string id,
            string version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        ///     Gets or sets the identifier of this plugin.
        /// </summary>
        [Required(ErrorMessage = "The plugin ID is required.")]
        public string Id { get; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        [Required(ErrorMessage = "The plugin version is required.")]
        public string Version { get; }
    }
}
