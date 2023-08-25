// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    internal record TransformedBase(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        bool Overwrite);
}