// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    /// <summary>
    ///    Represents the metadata of a plugin that is used during initialization.
    /// </summary>
    internal class PluginMetadataInit
    {
        /// <summary>
        ///     Gets or sets the identifer of this plugin.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        public Version? Version { get; set; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwner>? Owners { get; set; }

        /// <summary>
        ///     Gets or sets the version of the performance SDK which this plugin depends upon.
        /// </summary>
        public Version? SdkVersion { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the processing sources contained in this plugin.
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata>? ProcessingSources { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the data cookers contained in this plugin.
        /// </summary>
        public IEnumerable<DataCookerMetadata>? DataCookers { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the extensible tables contained in this plugin.
        /// </summary>
        public IEnumerable<TableMetadata>? ExtensibleTables { get; set; }

        /// <summary>
        ///     Converts this object to a <see cref="PluginMetadata"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="PluginMetadata"/> that represents this object.
        /// </returns>
        public PluginMetadata ToPluginMetadata()
        {
            return new PluginMetadata(
                this.Id,
                this.Version,
                this.DisplayName,
                this.Description,
                this.Owners,
                this.SdkVersion,
                this.ProcessingSources,
                this.DataCookers,
                this.ExtensibleTables);
        }

        /// <summary>
        ///     Creates a new <see cref="PluginMetadataInit"/> from the given <see cref="PluginMetadata"/>.
        /// </summary>
        /// <param name="pluginMetadata">
        ///     The <see cref="PluginMetadata"/> to convert.
        /// </param>
        /// <returns>
        ///     A new <see cref="PluginMetadataInit"/> that represents the given <see cref="PluginMetadata"/>.
        /// </returns>
        public static PluginMetadataInit FromPluginMetadata(PluginMetadata pluginMetadata)
        {
            return new PluginMetadataInit
            {
                Id = pluginMetadata.Id,
                Version = pluginMetadata.Version,
                DisplayName = pluginMetadata.DisplayName,
                Description = pluginMetadata.Description,
                Owners = pluginMetadata.Owners,
                SdkVersion = pluginMetadata.SdkVersion,
                ProcessingSources = pluginMetadata.ProcessingSources,
                DataCookers = pluginMetadata.DataCookers,
                ExtensibleTables = pluginMetadata.ExtensibleTables,
            };
        }
    }
}
