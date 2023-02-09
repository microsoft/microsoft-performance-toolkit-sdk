// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a table.
    /// </summary>
    public sealed class TableMetadata
    {
        /// <summary>
        ///     Gets or sets the unique identifier of this table.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Gets or sets the table name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the user friendly description of this table.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the category into which this table belongs.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this table is a metadata table.
        /// </summary>
        public bool IsMetadataTable{ get; set; }
    }
}
