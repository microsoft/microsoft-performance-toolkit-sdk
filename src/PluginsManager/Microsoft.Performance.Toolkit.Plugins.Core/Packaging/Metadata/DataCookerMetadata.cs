// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a data cooker.
    /// </summary>
    public sealed class DataCookerMetadata
    {
        /// <summary>
        ///     Gets or sets the name of this data cooker.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the user friendly description of this data cooker.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the Guid of the processing source this data cooker roots from.
        /// </summary>
        public Guid ProcessingSourceGuid { get; set; }
    }
}
