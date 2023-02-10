// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a processing source.
    /// </summary>
    public sealed class ProcessingSourceMetadata
    {
        /// <summary>
        ///     Gets or sets the unique identifier for this processing source.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Gets or sets the name of this processing source.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the version of this procesing source.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     Gets or sets the description of this processing source.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the information about this processing source as specified in <see cref="ProcessingSourceInfo"/>.
        /// </summary>
        public ProcessingSourceInfo AboutInfo { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the tables exposed by this processing source.
        /// </summary>
        public IEnumerable<TableMetadata> AvailableTables { get; set; }

        /// <summary>
        ///     Gets or sets the metadata of the data sources supported by this processing source.
        /// </summary>
        public IEnumerable<DataSourceMetadata> SupportedDataSources { get; set; }
    }
}
