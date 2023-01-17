// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a plugin
    /// </summary>
    public sealed class PluginMetadata
    {
        /// <summary>
        /// The identifer of this plugin
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The human-readable name of this plugin
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A user friendly description of this plugin
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The owners of this plugin
        /// </summary>
        public IEnumerable<PluginOwner> Owners { get; set; }

        /// <summary>
        /// The version of the performance SDK which this plugin depends upon
        /// </summary>
        public Version SdkVersion { get; set; }

        /// <summary>
        /// The metadata of the processing sources contained in this plugin
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; set; }

        /// <summary>
        /// The metadata of the data cookers contained in this plugin
        /// </summary>
        public IEnumerable<DataCookerMetadata> DataCookers { get; set; }

        /// <summary>
        /// The metadata of the extensible tables contained in this plugin
        /// </summary>
        public IEnumerable<TableMetadata> ExtensibleTables { get; set; }
    }
}
