// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Contains the information of an plugin recorded upon installation.
    /// </summary>
    public sealed class InstalledPluginInfo
        : IEquatable<InstalledPluginInfo>
    {
        [JsonConstructor]
        public InstalledPluginInfo(
            PluginIdentity identity,
            Uri sourceUri,
            string displayName,
            string description,
            DateTimeOffset installedOn,
            string checksum)
        {
            Guard.NotNull(identity, nameof(identity));
            Guard.NotNull(sourceUri, nameof(sourceUri));
            Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));
            Guard.NotNull(description, nameof(description));
            Guard.NotNullOrWhiteSpace(checksum, nameof(checksum));

            this.Identity = identity;
            this.SourceUri = sourceUri;
            this.DisplayName = displayName;
            this.Description = description;
            this.InstalledOn = installedOn;
            this.Checksum = checksum;
        }

        /// <summary>
        ///     Gets the identifier of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the identifier of this plugin.
        /// </summary>
        [JsonIgnore]
        public string Id
        {
            get
            {
                return this.Identity.Id;
            }
        }

        /// <summary>
        ///     Gets the version of this plugin.
        /// </summary>
        [JsonIgnore]
        public Version Version
        {
            get
            {
                return this.Identity.Version;
            }
        }

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
        ///     Gets the timestamp when the plugin is installed.
        /// </summary>
        public DateTimeOffset InstalledOn { get; }

        /// <summary>
        ///     Gets the checksum of the installed plugin.
        /// </summary>
        public string Checksum { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as InstalledPluginInfo);
        }

        /// <inheritdoc />
        public bool Equals(InstalledPluginInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Identity.Equals(other.Identity)
                && Equals(this.SourceUri, other.SourceUri)
                && string.Equals(this.DisplayName, other.DisplayName, StringComparison.Ordinal)
                && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                && DateTimeOffset.Equals(this.InstalledOn, other.InstalledOn)
                && string.Equals(this.Checksum, other.Checksum);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Identity.GetHashCode(),
                this.SourceUri.GetHashCode(),
                this.DisplayName.GetHashCode(),
                this.Description.GetHashCode(),
                this.InstalledOn.GetHashCode(),
                this.Checksum.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Id} - {this.Version}";
        }
    }
}
