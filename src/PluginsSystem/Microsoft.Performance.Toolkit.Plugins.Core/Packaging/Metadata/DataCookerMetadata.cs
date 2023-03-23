// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a data cooker.
    /// </summary>
    public sealed class DataCookerMetadata
    {
        /// <summary>
        ///     Initializes an instance of <see cref="DataCookerMetadata".s
        /// </summary>
        [JsonConstructor]
        public DataCookerMetadata(
            string name,
            string description,
            Guid processingSourceGuid)
        {
            this.Name = name;
            this.Description = description;
            this.ProcessingSourceGuid = processingSourceGuid;
        }

        /// <summary>
        ///     Gets the name of this data cooker.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the user friendly description of this data cooker.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the Guid of the processing source this data cooker roots from.
        /// </summary>
        public Guid ProcessingSourceGuid { get; }
    }
}
