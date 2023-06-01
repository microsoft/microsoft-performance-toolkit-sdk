// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a plugin.
    /// </summary>
    public sealed class PluginMetadata
        : IEquatable<PluginMetadata>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginMetadata"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public PluginMetadata(
            string id,
            Version version,
            ulong installedSize,
            string displayName,
            string description,
            IEnumerable<PluginOwner> owners,
            Version sdkVersion,
            IEnumerable<ProcessingSourceMetadata> processingSources,
            IEnumerable<DataCookerMetadata> dataCookers,
            IEnumerable<TableMetadata> extensibleTables)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNull(version, nameof(version));

            this.Id = id;
            this.Version = version;
            this.InstalledSize = installedSize;
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.SdkVersion = sdkVersion;
            this.ProcessingSources = processingSources;
            this.DataCookers = dataCookers;
            this.ExtensibleTables = extensibleTables;

            this.Identity = new PluginIdentity(id, version);
        }

        /// <summary>
        ///     Gets the identity of this processing source.
        /// </summary>
        [JsonIgnore]
        public PluginIdentity Identity { get; }

        /// <summary>
        ///     Gets the identifier of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets the version of this plugin.
        /// </summary>
        public Version Version { get; }

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
        ///     Gets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwner> Owners { get; }

        /// <summary>
        ///     Gets the version of the performance SDK which this plugin depends upon.
        /// </summary>
        public Version SdkVersion { get; }

        /// <summary>
        ///     Gets the metadata of the processing sources contained in this plugin.
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; }

        /// <summary>
        ///     Gets the metadata of the data cookers contained in this plugin.
        /// </summary>
        public IEnumerable<DataCookerMetadata> DataCookers { get; }

        /// <summary>
        ///     Gets the metadata of the extensible tables contained in this plugin.
        /// </summary>
        public IEnumerable<TableMetadata> ExtensibleTables { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as PluginMetadata);
        }

        /// <inheritdoc/>
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

            return string.Equals(this.Id, other.Id, StringComparison.Ordinal)
                && this.Version.Equals(other.Version)
                && this.InstalledSize.Equals(other.InstalledSize)
                && string.Equals(this.DisplayName, other.DisplayName, StringComparison.Ordinal)
                && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                && this.Owners.EnumerableEqual(other.Owners)
                && this.SdkVersion?.Equals(other.SdkVersion) == true
                && this.ProcessingSources.EnumerableEqual(other.ProcessingSources)
                && this.DataCookers.EnumerableEqual(other.DataCookers)
                && this.ExtensibleTables.EnumerableEqual(other.ExtensibleTables);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.Id?.GetHashCode() ?? 0,
                this.Version?.GetHashCode() ?? 0,
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

            if (this.ProcessingSources != null)
            {
                foreach (ProcessingSourceMetadata source in this.ProcessingSources)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, source?.GetHashCode() ?? 0);
                }
            }

            if (this.DataCookers != null)
            {
                foreach (DataCookerMetadata cooker in this.DataCookers)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, cooker?.GetHashCode() ?? 0);
                }
            }

            if (this.ExtensibleTables != null)
            {
                foreach (TableMetadata table in this.ExtensibleTables)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, table?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
