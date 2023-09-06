// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Represents the manifest for a plugin.
    /// </summary>
    public sealed class PluginManifest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManifest"/>s
        /// </summary>
        /// <param name="identity">
        ///     The identity of the plugin.
        /// </param>
        /// <param name="displayName">
        ///     The human-readable name of this plugin.
        /// </param>
        /// <param name="description">
        ///     The user friendly description of this plugin.
        /// </param>
        /// <param name="owners">
        ///     The owners of this plugin.
        /// </param>
        /// <param name="projectUrl">
        ///     The URL of the project that owns this plugin.
        /// </param>
        public PluginManifest(
            PluginIdentityManifest identity,
            string displayName,
            string description,
            IEnumerable<PluginOwnerInfoManifest> owners,
            Uri projectUrl)
        {
            this.Identity = identity;
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.ProjectUrl = projectUrl;
        }
        
        /// <summary>
        ///     Gets the identity of the plugin.
        /// </summary>
        public PluginIdentityManifest Identity { get; }

        /// <summary>
        ///     Gets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets the user friendly description of this plugin.
        /// </summary>
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwnerInfoManifest> Owners { get; }

        /// <summary>
        ///     Gets the URL of the project that owns this plugin.
        /// </summary>
        public Uri ProjectUrl { get; }
    }
}
