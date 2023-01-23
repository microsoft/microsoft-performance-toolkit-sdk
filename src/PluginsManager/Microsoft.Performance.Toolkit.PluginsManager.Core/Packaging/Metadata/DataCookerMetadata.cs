// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a data cooker
    /// </summary>
    public sealed class DataCookerMetadata
    {
        /// <summary>
        /// The name of this data cooker
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A user friendly description of this data cooker
        /// </summary>
        public string Description { get; set; }
    }
}
