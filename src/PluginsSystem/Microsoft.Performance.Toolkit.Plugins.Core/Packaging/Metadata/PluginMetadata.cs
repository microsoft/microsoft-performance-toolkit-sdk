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
        private readonly Lazy<PluginIdentity> identity;

        /// <summary>
        ///     Intializes a new instance of the <see cref="PluginMetadata"/> class with the specified parameters.
        /// </summary>
        [JsonConstructor]
        public PluginMetadata(
            string id,
            Version version,
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
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.SdkVersion = sdkVersion;
            this.ProcessingSources = processingSources;
            this.DataCookers = dataCookers;
            this.ExtensibleTables = extensibleTables;

            this.identity = new Lazy<PluginIdentity>(() => new PluginIdentity(id, version));
        }

        /// <summary>
        ///     Gets or sets the identity of this processing source.
        /// </summary>
        [JsonIgnore]
        public PluginIdentity Identity
        {
            get
            {
                return this.identity.Value;
            }
        }

        /// <summary>
        ///     Gets or sets the identifer of this plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwner> Owners { get; }

        /// <summary>
        ///     Gets or sets the version of the performance SDK which this plugin depends upon.
        /// </summary>
        public Version SdkVersion { get; }

        /// <summary>
        ///     Gets or sets the metadata of the processing sources contained in this plugin.
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; }

        /// <summary>
        ///     Gets or sets the metadata of the data cookers contained in this plugin.
        /// </summary>
        public IEnumerable<DataCookerMetadata> DataCookers { get; }

        /// <summary>
        ///     Gets or sets the metadata of the extensible tables contained in this plugin.
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
                this.Id.GetHashCode(),
                this.Version.GetHashCode(),
                this.DisplayName?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.SdkVersion?.GetHashCode() ?? 0);

            if (this.Owners != null)
            {
                foreach (PluginOwner owner in this.Owners)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, owner.GetHashCode());
                }
            }

            if (this.ProcessingSources != null)
            {
                foreach (ProcessingSourceMetadata source in this.ProcessingSources)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, source.GetHashCode());
                }
            }

            if (this.DataCookers != null)
            {
                foreach (DataCookerMetadata cooker in this.DataCookers)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, cooker.GetHashCode());
                }
            }

            if (this.ExtensibleTables != null)
            {
                foreach (TableMetadata table in this.ExtensibleTables)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, table.GetHashCode());
                }
            }

            return result;
        }
    }
}
