// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a supported data source.
    /// </summary>
    public sealed class DataSourceMetadata
    {
        /// <summary>
        ///     Gets or sets the name of this data source.
        ///     If a <see cref="FileDataSource"/>, use the file extension as name (e.g. ".etl")
        ///     If a <see cref="DirectoryDataSource"/>, use "directory" as name
        ///     If a <see cref="ExtensionlessFileDataSourceAttribute"/>, use "extensionless" as name
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        ///     Gets or sets the description of this data source.
        /// </summary>
        public string Description { get; set; }
    }
};
