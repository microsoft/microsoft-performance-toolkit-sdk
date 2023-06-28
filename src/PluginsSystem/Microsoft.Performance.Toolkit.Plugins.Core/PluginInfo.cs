// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    ///     Represents the information/metadata of a plugin.
    /// </summary>
    public class PluginInfo
        : IEquatable<PluginInfo>
    {
        /// <summary>
        ///     Initializes an instance of <see cref="AvailablePluginInfo"/>.
        /// </summary>
        [JsonConstructor]
        public PluginInfo(
            PluginIdentity identity,
            ulong installedSize,
            string displayName,
            string description,
            Version sdkVersion,
            IEnumerable<PluginOwner> owners)
        {
            Identity = identity;
            InstalledSize = installedSize;
            DisplayName = displayName;
            Description = description;
            SdkVersion = sdkVersion;
            Owners = owners;
        }

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
        public Version SdkVersion { get; }

        /// <summary>
        ///     Gets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwner> Owners { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as PluginInfo);
        }

        /// <inheritdoc />
        public bool Equals(PluginInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Identity.Equals(other.Identity)
                   && this.InstalledSize.Equals(other.InstalledSize)
                   && this.DisplayName.Equals(other.DisplayName)
                   && this.Description.Equals(other.Description)
                   && this.SdkVersion.Equals(other.SdkVersion)
                   && this.Owners.EnumerableEqual(other.Owners);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.Identity?.GetHashCode() ?? 0,
                this.InstalledSize.GetHashCode(),
                this.DisplayName?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.SdkVersion?.GetHashCode() ?? 0);

            if (this.Owners != null)
            {
                foreach (PluginOwner owner in this.Owners)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, owner?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
