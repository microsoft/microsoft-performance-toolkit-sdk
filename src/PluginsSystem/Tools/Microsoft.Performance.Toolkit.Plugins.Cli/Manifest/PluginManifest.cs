// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public sealed class PluginManifest
    {
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
        
        public PluginIdentityManifest Identity { get; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwnerInfoManifest> Owners { get; }

        /// <summary>
        ///     Gets or sets the URL of the project that owns this plugin.
        /// </summary>
        public Uri ProjectUrl { get; }
    }
}
