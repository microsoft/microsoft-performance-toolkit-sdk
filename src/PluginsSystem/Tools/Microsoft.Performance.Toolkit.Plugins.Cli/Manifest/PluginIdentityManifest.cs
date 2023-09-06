// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Represents the identity of a plugin in a manifest.
    /// </summary>
    public sealed class PluginIdentityManifest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginIdentityManifest"/>
        /// </summary>
        /// <param name="id">
        ///     The identifier of this plugin.
        /// </param>
        /// <param name="version">
        ///     The version of this plugin.
        /// </param>
        public PluginIdentityManifest(
            string id,
            Version version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        ///     Gets or sets the identifier of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        public Version Version { get; }
    }
}
