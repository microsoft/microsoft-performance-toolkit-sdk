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
            PluginIdentity identity,
            PluginSource source,
            ulong size,
            string displayName,
            string description,
            Uri packageUri,
            Guid fetcherResourceId)
        {
            Guard.NotNull(identity, nameof(identity));
            Guard.NotNull(source, nameof(source));
            Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));
            Guard.NotNull(description, nameof(description));
            Guard.NotNull(packageUri, nameof(packageUri));

            this.Identity = identity;
            this.Source = source;
            this.Size = size;
            this.DisplayName = displayName;
            this.Description = description;
            this.PackageUri = packageUri;
            this.FetcherResourceId = fetcherResourceId;
        }

        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the source where this plugin is discovered.
        /// </summary>
        public PluginSource Source { get; }

        /// <summary>
        ///     Gets the number of bytes that constitute this plugin.
        /// </summary>
        public ulong Size { get; }

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
        public Uri PackageUri { get; }

        /// <summary>
        ///     Gets the Guid which identifies the unique <see cref="Transport.IPluginFetcher"/> resource
        ///     the plugin package should be fetched from.
        /// </summary>
        public Guid FetcherResourceId { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as AvailablePluginInfo);
        }

        /// <inheritdoc />
        public bool Equals(AvailablePluginInfo other)
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
                && this.Source.Equals(other.Source)
                && this.Size.Equals(other.Size)
                && this.DisplayName.Equals(other.DisplayName, StringComparison.Ordinal)
                && this.Description.Equals(other.Description, StringComparison.Ordinal)
                && this.PackageUri.Equals(other.PackageUri)
                && this.FetcherResourceId.Equals(other.FetcherResourceId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Identity.GetHashCode(),
                this.Source.GetHashCode(),
                this.DisplayName.GetHashCode(),
                this.Description.GetHashCode(),
                this.PackageUri.GetHashCode(),
                this.FetcherResourceId.GetHashCode());
        }
    }
}
