// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a table
    /// </summary>
    public sealed class TableMetadata
    {
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
    }
}
