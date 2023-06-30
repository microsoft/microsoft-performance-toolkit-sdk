// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Represents the metadata of a processing source.
    /// </summary>
    public sealed class ProcessingSourceMetadata
        : IEquatable<ProcessingSourceMetadata>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceMetadata"/> class.
        /// </summary>
        [JsonConstructor]
        public ProcessingSourceMetadata(
            Guid guid,
            string name,
            Version version,
            string description,
            ProcessingSourceInfo aboutInfo,
            IEnumerable<TableMetadata> availableTables,
            IEnumerable<DataSourceMetadata> supportedDataSources)
        {
            this.Guid = guid;
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.AboutInfo = aboutInfo;
            this.AvailableTables = availableTables;
            this.SupportedDataSources = supportedDataSources;
        }
        /// <summary>
        ///     Gets or sets the unique identifier for this processing source.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets or sets the name of this processing source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the version of this procesing source.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Gets or sets the description of this processing source.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the information about this processing source as specified in <see cref="ProcessingSourceInfo"/>.
        /// </summary>
        public ProcessingSourceInfo AboutInfo { get; }

        /// <summary>
        ///     Gets or sets the metadata of the tables exposed by this processing source.
        /// </summary>
        public IEnumerable<TableMetadata> AvailableTables { get; }

        /// <summary>
        ///     Gets or sets the metadata of the data sources supported by this processing source.
        /// </summary>
        public IEnumerable<DataSourceMetadata> SupportedDataSources { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ProcessingSourceMetadata);
        }

        /// <inheritdoc/>
        public bool Equals(ProcessingSourceMetadata other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Guid.Equals(other.Guid)
                && string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && this.Version?.Equals(other.Version) == true
                && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                && Equals(this.AboutInfo, other.AboutInfo)
                && this.AvailableTables.EnumerableEqual(other.AvailableTables)
                && this.SupportedDataSources.EnumerableEqual(other.SupportedDataSources);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int result = HashCodeUtils.CombineHashCodeValues(
                this.Guid.GetHashCode(),
                this.Name?.GetHashCode() ?? 0,
                this.Version?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.AboutInfo?.GetHashCode() ?? 0);

            if (this.AvailableTables != null)
            {
                foreach (TableMetadata table in this.AvailableTables)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, table?.GetHashCode() ?? 0);
                }
            }

            if (this.SupportedDataSources != null)
            {
                foreach (DataSourceMetadata dataSource in this.SupportedDataSources)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, dataSource?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}
