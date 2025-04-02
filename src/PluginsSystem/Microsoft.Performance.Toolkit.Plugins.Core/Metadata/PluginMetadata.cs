// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Represents the information/metadata of a plugin.
    /// </summary>
    public class PluginMetadata
        : IEquatable<PluginMetadata>
    {
        /// <summary>
        ///     Initializes an instance of <see cref="PluginMetadata"/>.
        /// </summary>
        /// <param name="identity">
        ///     The identity of the plugin.
        /// </param>
        /// <param name="installedSize">
        ///     The size, in number of bytes, of this plugin once it has been installed.
        /// </param>
        /// <param name="displayName">
        ///     The human-readable name of this plugin.
        /// </param>
        /// <param name="description">
        ///     The user friendly description of this plugin.
        /// </param>
        /// <param name="sdkVersion">
        ///     The version of the performance SDK which this plugin depends upon.
        /// </param>
        /// <param name="projectUrl">
        ///     The project URL of this plugin.
        /// </param>
        /// <param name="owners">
        ///     The owners information of this plugin.
        /// </param>
        [JsonConstructor]
        public PluginMetadata(
            PluginIdentity identity,
            ulong installedSize,
            string displayName,
            string description,
            SemanticVersion sdkVersion,
            Uri projectUrl,
            IEnumerable<PluginOwnerInfo> owners)
        {
            this.Identity = identity;
            this.InstalledSize = installedSize;
            this.DisplayName = displayName;
            this.Description = description;
            this.SdkVersion = sdkVersion;
            this.ProjectUrl = projectUrl;
            this.Owners = owners;
        }

        /// <summary>
        ///     Gets the schema version of plugin metadata.
        /// </summary>
        public double SchemaVersion { get; } = 0.1;

        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the size, in number of bytes, of this plugin once it has been installed.
        /// </summary>
        public ulong InstalledSize { get; }

        /// <summary>
        ///     Gets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets the user friendly description of this plugin.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the version of the performance SDK which this plugin depends upon.
        /// </summary>
        public SemanticVersion SdkVersion { get; }

        /// <summary>
        ///     Gets the owners information of this plugin.
        /// </summary>
        public IEnumerable<PluginOwnerInfo> Owners { get; }

        /// <summary>
        ///     Gets the project URL of this plugin.
        /// </summary>
        public Uri ProjectUrl { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as PluginMetadata);
        }

        /// <inheritdoc />
        public bool Equals(PluginMetadata other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.SchemaVersion.Equals(other.SchemaVersion)
                   && (this.Identity?.Equals(other.Identity) == true || this.Identity is null && other.Identity is null)
                   && this.InstalledSize.Equals(other.InstalledSize)
                   && string.Equals(this.DisplayName, other.DisplayName, StringComparison.Ordinal)
                   && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                   && (this.SdkVersion?.Equals(other.SdkVersion) == true || this.SdkVersion is null && other.SdkVersion is null)
                   && (this.ProjectUrl?.Equals(other.ProjectUrl) == true || this.ProjectUrl is null && other.ProjectUrl is null)
                   && (this.Owners?.EnumerableEqual(other.Owners) == true || this.Owners is null && other.Owners is null);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.SchemaVersion.GetHashCode(),
                this.Identity?.GetHashCode() ?? 0,
                this.InstalledSize.GetHashCode(),
                this.DisplayName?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.SdkVersion?.GetHashCode() ?? 0,
                this.ProjectUrl?.GetHashCode() ?? 0);

            if (this.Owners != null)
            {
                foreach (PluginOwnerInfo owner in this.Owners)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, owner?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
