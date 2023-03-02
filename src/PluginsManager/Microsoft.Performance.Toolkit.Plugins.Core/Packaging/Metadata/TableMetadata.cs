// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a table.
    /// </summary>
    public sealed class TableMetadata
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
        public bool IsMetadataTable{ get; }
    }
}
