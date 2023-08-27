// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal record MetadataGenArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputDirectoryFullPath,
        bool Overwrite)
        : PackGenCommonArgs(
            SourceDirectoryFullPath,
            ManifestFileFullPath,
            Overwrite)
    {
        public MetadataGenArgs(PackGenCommonArgs baseOptions, string? outputDirectoryFullPath)
            : this(baseOptions.SourceDirectoryFullPath, baseOptions.ManifestFileFullPath, outputDirectoryFullPath, baseOptions.Overwrite)
        {
        }
    }
}
