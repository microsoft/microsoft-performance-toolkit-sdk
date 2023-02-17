// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a supported data source.
    /// </summary>
    public sealed class DataSourceMetadata
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceMetadata"/> class.
        /// </summary>
        [JsonConstructor]
        public DataSourceMetadata(
            string name,
            string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        ///     Gets the name of this data source.
        ///     If a <see cref="FileDataSource"/>, use the file extension as name (e.g. ".etl")
        ///     If a <see cref="DirectoryDataSource"/>, use "directory" as name
        ///     If a <see cref="ExtensionlessFileDataSourceAttribute"/>, use "extensionless" as name
        /// </summary>
        public string Name { get; }

        /// <summary> 
        ///     Gets the description of this data source.
        /// </summary>
        public string Description { get; }
    }
}
