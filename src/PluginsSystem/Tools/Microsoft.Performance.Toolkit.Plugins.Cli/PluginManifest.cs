using System.ComponentModel.DataAnnotations;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public sealed class PluginManifest
    {
        public PluginManifest(
            string id,
            string version,
            string displayName,
            string description,
            IEnumerable<PluginOwner> owners,
            string projectUrl)
        {
            this.Id = id;
            this.Version = version;
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.ProjectUrl = projectUrl;
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
        public IEnumerable<PluginOwner> Owners { get; }

        /// <summary>
        ///     Gets or sets the URL of the project that owns this plugin.
        /// </summary>
        public string ProjectUrl { get; }
}
}
