// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata
{
    /// <summary>
    /// Represents the metadata of a processing source.
    /// </summary>
    public class ProcessingSourceMetadata
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public ProcessingSourceInfo AboutInfo { get; set; }

        public IEnumerable<TableMetadata> AvailableTables { get; set; }

        public IEnumerable<SupportedFileDataSource> SupportedFileDataSources { get; set; }

        public IEnumerable<SupportedFolderDataSource> SupportedFolderDataSources { get; set; }

        public IEnumerable<SupportedExtensionlessFileDataSource> SupportedExtensionlessFileDataSources { get; set; }
    }
}
