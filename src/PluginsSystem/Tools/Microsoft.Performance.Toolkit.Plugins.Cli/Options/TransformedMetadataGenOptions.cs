// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    internal record TransformedMetadataGenOptions(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputDirectoryFullPath,
        bool Overwrite)
        : TransformedBase(
            SourceDirectoryFullPath,
            ManifestFileFullPath,
            Overwrite)
    {
        public TransformedMetadataGenOptions(TransformedBase baseOptions, string? outputDirectoryFullPath)
            : this(baseOptions.SourceDirectoryFullPath, baseOptions.ManifestFileFullPath, outputDirectoryFullPath, baseOptions.Overwrite)
        {
        }
    }
}
