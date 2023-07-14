using System.ComponentModel.DataAnnotations;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal sealed class PluginManifest
    {
        public PluginManifest(
            PluginIdentityManifest identity,
            string displayName,
            string description,
            IEnumerable<PluginOwnerInfoManifest> owners,
            string projectUrl)
        {
            this.Identity = identity;
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.ProjectUrl = projectUrl;
        }

        /// <summary>
        ///     Gets or sets the identity of this plugin.
        /// </summary>
        [Required(ErrorMessage = "The plugin identity is required.")]
        public PluginIdentityManifest Identity { get; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        [Required(ErrorMessage = "The plugin display name is required.")]
        public string DisplayName { get; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>
        [Required(ErrorMessage = "The plugin description is required.")]
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwnerInfoManifest> Owners { get; }

        /// <summary>
        ///     Gets or sets the URL of the project that owns this plugin.
        /// </summary>
        public string ProjectUrl { get; }
    }
}
