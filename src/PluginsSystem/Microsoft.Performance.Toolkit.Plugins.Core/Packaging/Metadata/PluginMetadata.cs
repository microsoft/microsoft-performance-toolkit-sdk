// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a plugin.
    /// </summary>
    public sealed class PluginMetadata
    {
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
            this.Id = id;
            this.Version = version;
            this.DisplayName = displayName;
            this.Description = description;
            this.Owners = owners;
            this.SdkVersion = sdkVersion;
            this.ProcessingSources = processingSources;
            this.DataCookers = dataCookers;
            this.ExtensibleTables = extensibleTables;
        }

        /// <summary>
        ///     Intializes a new instance of the <see cref="PluginMetadata"/> class.
        /// </summary>
        public PluginMetadata()
        {  
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
    }
}
