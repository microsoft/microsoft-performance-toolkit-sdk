﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a plugin.
    /// </summary>
    public sealed class PluginMetadata
    {
        public static bool TryParse(Stream jsonStream, out PluginMetadata pluginMetadata)
        {
            Guard.NotNull(jsonStream, nameof(jsonStream));

            // TODO: #238 Error handling
            try
            {
                using (var doc = JsonDocument.Parse(jsonStream))
                {
                    pluginMetadata = JsonSerializer.Deserialize<PluginMetadata>(jsonStream);
                    return pluginMetadata != null;
                }
            }
            catch
            {
                pluginMetadata = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets or sets the identifer of this plugin.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the version of this plugin.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     Gets or sets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the user friendly description of this plugin.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the owners of this plugin.
        /// </summary>
        public IEnumerable<PluginOwner> Owners { get; set; }

        /// <summary>
        ///     Gets or sets the version of the performance SDK which this plugin depends upon.
        /// </summary>
        public Version SdkVersion { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the processing sources contained in this plugin.
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the data cookers contained in this plugin.
        /// </summary>
        public IEnumerable<DataCookerMetadata> DataCookers { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the extensible tables contained in this plugin.
        /// </summary>
        public IEnumerable<TableMetadata> ExtensibleTables { get; set; }
    }
}