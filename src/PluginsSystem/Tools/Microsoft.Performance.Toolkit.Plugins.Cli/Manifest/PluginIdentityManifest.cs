// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public class PluginIdentityManifest
    {
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
