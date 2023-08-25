// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    internal record TransformedPackOptions(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputFileFullPath,
        bool OverWrite)
        : TransformedBase(SourceDirectoryFullPath, ManifestFileFullPath, OverWrite)
    {
        public TransformedPackOptions(TransformedBase baseOptions, string? outputFileFullPath)
            : this(baseOptions.SourceDirectoryFullPath, baseOptions.ManifestFileFullPath, outputFileFullPath, baseOptions.Overwrite)
        {
        }
    }
}
