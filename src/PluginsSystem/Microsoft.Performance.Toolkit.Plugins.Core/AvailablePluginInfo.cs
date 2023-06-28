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
            PluginInfo pluginInfo,
            PluginSource source,
            ulong packageSize,
            Uri packageUri,
            Guid fetcherResourceId)
        {
            Guard.NotNull(pluginInfo, nameof(pluginInfo));
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(packageUri, nameof(packageUri));

            this.PluginInfo = pluginInfo;
            this.Source = source;
            this.PackageSize = packageSize;
            this.PackageUri = packageUri;
            this.FetcherResourceId = fetcherResourceId;
        }

        /// <summary>
        ///     Gets the information for this plugin.
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        ///     Gets the source where this plugin is discovered.
        /// </summary>
        public PluginSource Source { get; }

        /// <summary>
        ///     Gets the size, in number of bytes, of the package that makes up this plugin.
        /// </summary>
        public ulong PackageSize { get; }


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

            return this.PluginInfo.Equals(other.PluginInfo)
                && this.Source.Equals(other.Source)
                && this.PackageSize.Equals(other.PackageSize)
                && this.PackageUri.Equals(other.PackageUri)
                && this.FetcherResourceId.Equals(other.FetcherResourceId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.PluginInfo.GetHashCode(),
                this.Source.GetHashCode(),
                this.PackageSize.GetHashCode(),
                this.PackageUri.GetHashCode(),
                this.FetcherResourceId.GetHashCode());
        }
    }
}
