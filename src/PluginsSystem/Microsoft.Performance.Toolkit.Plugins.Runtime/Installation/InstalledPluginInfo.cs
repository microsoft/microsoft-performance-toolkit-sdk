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
            PluginInfo pluginInfo,
            Uri sourceUri,
            DateTimeOffset installedOn,
            string checksum)
        {
            Guard.NotNull(pluginInfo, nameof(pluginInfo));
            Guard.NotNull(sourceUri, nameof(sourceUri));
            Guard.NotNullOrWhiteSpace(checksum, nameof(checksum));

            this.PluginInfo = pluginInfo;
            this.SourceUri = sourceUri;
            this.InstalledOn = installedOn;
            this.Checksum = checksum;
        }

        /// <summary>
        ///     Gets the information for this plugin.
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        ///     Gets the source Uri of this plugin.
        /// </summary>
        public Uri SourceUri { get; }

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

            return this.PluginInfo.Equals(other.PluginInfo)
                && Equals(this.SourceUri, other.SourceUri)
                && DateTimeOffset.Equals(this.InstalledOn, other.InstalledOn)
                && string.Equals(this.Checksum, other.Checksum);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.PluginInfo.GetHashCode(),
                this.SourceUri.GetHashCode(),
                this.InstalledOn.GetHashCode(),
                this.Checksum.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.PluginInfo.Identity.ToString();
        }
    }
}
