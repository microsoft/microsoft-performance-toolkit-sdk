// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Core
{
    /// <summary>
    ///     A DTO that contains information of a discovered plugin that is available for installation.
    /// </summary>
    public sealed class AvailablePluginInfo
        : IEquatable<AvailablePluginInfo>
    {
        /// <summary>
        ///     Initializes an instance of <see cref="AvailablePluginInfo"/>.
        /// </summary>
        [JsonConstructor]
        public AvailablePluginInfo(
            PluginIdentity Identity,
            PluginSource pluginSource,
            string displayName,
            string description,
            Uri pluginPackageUri,
            Guid fetcherResourceId)
        {
            this.Identity = Identity;
            this.PluginSource = pluginSource;
            this.DisplayName = displayName;
            this.Description = description;
            this.PluginPackageUri = pluginPackageUri;
            this.FetcherResourceId = fetcherResourceId;
        }

        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the souce where this plugin is discovered.
        /// </summary>
        public PluginSource PluginSource { get; }

        /// <summary>
        ///     Gets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets the user friendly description of this plugin.
        /// </summary>   
        public string Description { get; }

        /// <summary>
        ///     Gets the URI where the plugin package can be fetched.
        /// </summary>
        public Uri PluginPackageUri { get; }

        /// <summary>
        ///     Gets the Guid which identifies the unique <see cref="Transport.IPluginFetcher"/> resouce
        ///     the plugin package should be fetched from.
        /// </summary>
        public Guid FetcherResourceId { get; }

        /// <inheritdoc />
        public bool Equals(AvailablePluginInfo other)
        {
            return Equals(this, other);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="AvailablePluginInfo"/> instances are considered equal.
        /// </summary>
        /// <param name="a">
        ///     The first available plugin info to compare.  
        /// </param>
        /// <param name="b">
        ///     The second available plugin info to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a and b are considered equal; <c>false</c> otherwise.
        ///     If a or b is null, the method returns <c>false</c>.
        /// </returns>
        public static bool Equals(AvailablePluginInfo a, AvailablePluginInfo b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Identity.Equals(b.Identity) &&
                a.PluginSource.Equals(b.PluginSource) &&
                a.DisplayName.Equals(b.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                a.Description.Equals(b.Description, StringComparison.OrdinalIgnoreCase) &&
                a.PluginPackageUri.Equals(b.PluginPackageUri) &&
                a.FetcherResourceId.Equals(b.FetcherResourceId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Identity.GetHashCode(),
                this.PluginSource.GetHashCode(),
                this.DisplayName.GetHashCode(),
                this.Description.GetHashCode(),
                this.PluginPackageUri.GetHashCode(),
                this.FetcherResourceId.GetHashCode());
        }
    }
}
