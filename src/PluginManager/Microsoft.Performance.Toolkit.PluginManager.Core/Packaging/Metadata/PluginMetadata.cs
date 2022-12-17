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
        /// Plugin Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The display name of this plugin
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A description of what this plugin is
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Metadata of the processing sources contained in this plugin
        /// </summary>
        public IEnumerable<ProcessingSourceMetadata> ProcessingSourceMetadataCollection { get; set; }
    }
}
