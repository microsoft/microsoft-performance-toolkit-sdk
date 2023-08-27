// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal record PackArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputFileFullPath,
        bool OverWrite)
        : PackGenCommonArgs(SourceDirectoryFullPath, ManifestFileFullPath, OverWrite)
    {
        public PackArgs(PackGenCommonArgs baseOptions, string? outputFileFullPath)
            : this(baseOptions.SourceDirectoryFullPath, baseOptions.ManifestFileFullPath, outputFileFullPath, baseOptions.Overwrite)
        {
        }
    }
}
