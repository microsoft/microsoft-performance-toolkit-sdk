// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a plugin. 
    /// </summary>
    public sealed class PluginMetadata
    {
        /// <summary>
        /// The group this plugin belongs to
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The identifer of the plugin
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The version of the plugin
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
        /// The version of the performance SDK which this plugin depends upon
        /// </summary>
        public Version SdkVersion { get; set; }

        /// <summary>
        /// The architecture of the platforms targeted by the plugin
        /// </summary>
        public Architecture[] TargetPlatforms { get; set; }

        /// <summary>
        /// The metadata of the processing sources contained in this plugin
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSourceMetadataCollection { get; set; }
    }
}
