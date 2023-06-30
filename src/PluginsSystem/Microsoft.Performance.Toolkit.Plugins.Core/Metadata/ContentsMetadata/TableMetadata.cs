// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Metadata
{
    /// <summary>
    ///     Represents the metadata of a table.
    /// </summary>
    public sealed class TableMetadata
        : IEquatable<TableMetadata>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableMetadata"/> class.
        /// </summary>
        [JsonConstructor]
        public TableMetadata(
            Guid guid,
            string name,
            string description,
            string category,
            bool isMetadataTable)
        {
            this.Guid = guid;
            this.Name = name;
            this.Description = description;
            this.Category = category;
            this.IsMetadataTable = isMetadataTable;
        }
        /// <summary>
        ///     Gets or sets the unique identifier of this table.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets or sets the table name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the user friendly description of this table.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the category into which this table belongs.
        /// </summary>
        public string Category { get; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this table is a metadata table.
        /// </summary>
        public bool IsMetadataTable { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as TableMetadata);
        }

        /// <inheritdoc />
        public bool Equals(TableMetadata other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Guid.Equals(other.Guid)
                && string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && string.Equals(this.Description, other.Description, StringComparison.Ordinal)
                && string.Equals(this.Category, other.Category, StringComparison.Ordinal)
                && this.IsMetadataTable.Equals(other.IsMetadataTable);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Guid.GetHashCode(),
                this.Name?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0,
                this.Category?.GetHashCode() ?? 0,
                this.IsMetadataTable.GetHashCode());
        }
    }
}
