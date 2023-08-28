// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal record PackGenCommonArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        bool Overwrite);
}
