// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Installation
{
    /// <summary>
    ///     Contains the information of an plugin recorded upon installation.
    /// </summary>
    public sealed class InstalledPluginInfo
        : IEquatable<InstalledPluginInfo>
    {
        [JsonConstructor]
        public InstalledPluginInfo(
            string id,
            Version version,
            Uri sourceUri,
            string displayName,
            string description,
            string installPath,
            DateTimeOffset installedOn,
            string checksum)
        {
            this.Id = id;
            this.Version = version;
            this.SourceUri = sourceUri;
            this.DisplayName = displayName;
            this.Description = description;
            this.InstallPath = installPath;
            this.InstalledOn = installedOn;
            this.Checksum = checksum;
        }

        /// <summary>
        ///     Gets the identifier of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets the version of this plugin.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Gets the source Uri of this plugin.
        /// </summary>
        public Uri SourceUri { get; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>   
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the path to where the plugin is installed.
        /// </summary>
        public string InstallPath { get; }

        /// <summary>
        ///     Gets the timestamp when the plugin is installed.
        /// </summary>
        public DateTimeOffset InstalledOn { get; }

        /// <summary>
        ///     Gets the checksum of the installed plugin.
        /// </summary>
        public string Checksum { get; }

        /// <inheritdoc />
        public bool Equals(InstalledPluginInfo other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return string.Equals(this.Id, this.Id, StringComparison.OrdinalIgnoreCase) &&
                   Equals(this.Version, other.Version) &&
                   Equals(this.SourceUri, other.SourceUri) &&
                   string.Equals(this.DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.InstallPath, other.InstallPath, StringComparison.OrdinalIgnoreCase) &&
                   DateTimeOffset.Equals(this.InstalledOn, other.InstalledOn) &&
                   string.Equals(this.Checksum, other.Checksum);
        }
    }
}
