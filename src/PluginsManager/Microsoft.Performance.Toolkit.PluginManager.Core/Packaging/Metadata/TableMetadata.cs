// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a table
    /// </summary>
    public sealed class TableMetadata
    {
        /// <summary>
        /// The unique identifier of this table
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A user friendly description of this table
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The category into which this table belongs
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Indicates whether this table is a metadata table
        /// </summary>
        public bool IsMetadataTable{ get; set; }
    }
}
