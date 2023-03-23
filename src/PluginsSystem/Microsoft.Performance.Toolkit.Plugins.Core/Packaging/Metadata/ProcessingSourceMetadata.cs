// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents the metadata of a processing source.
    /// </summary>
    public sealed class ProcessingSourceMetadata
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceMetadata"/> class.
        /// </summary>
        [JsonConstructor]
        public ProcessingSourceMetadata(
            Guid guid,
            string name,
            Version version,
            string description,
            ProcessingSourceInfo aboutInfo,
            IEnumerable<TableMetadata> availableTables,
            IEnumerable<DataSourceMetadata> supportedDataSources)
        {
            this.Guid = guid;
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.AboutInfo = aboutInfo;
            this.AvailableTables = availableTables;
            this.SupportedDataSources = supportedDataSources;
        }
        /// <summary>
        ///     Gets or sets the unique identifier for this processing source.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets or sets the name of this processing source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the version of this procesing source.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Gets or sets the description of this processing source.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets the information about this processing source as specified in <see cref="ProcessingSourceInfo"/>.
        /// </summary>
        public ProcessingSourceInfo AboutInfo { get; }

        /// <summary>
        ///     Gets or sets the metadata of the tables exposed by this processing source.
        /// </summary>
        public IEnumerable<TableMetadata> AvailableTables { get; }

        /// <summary>
        ///     Gets or sets the metadata of the data sources supported by this processing source.
        /// </summary>
        public IEnumerable<DataSourceMetadata> SupportedDataSources { get; }
    }
}
