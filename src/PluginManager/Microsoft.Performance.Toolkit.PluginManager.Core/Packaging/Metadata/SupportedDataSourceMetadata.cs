// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata
{
    public sealed class SupportedDataSourceMetadata
    {
        /// <summary>
        /// The type of the data source
        /// </summary>
        public DataSourceType DataSourceType { get; set; }

        /// <summary>
        /// The description of the data source
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The supported file extension if this data source is a <see cref="DataSourceType.FileDataSource"/>
        /// </summary>
        /// TODO: Add base class
        public string Extension { get; set; }
    }
}
