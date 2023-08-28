﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Processing
{
    /// <summary>
    ///     Represents the artifacts of a plugin.
    /// </summary>
    /// <param name="SourceDirectoryFullPath">
    ///     The full path to the directory containing the plugin binaries.
    /// </param>
    /// <param name="ManifestFileFullPath">
    ///     The full path to the manifest file, if it's not in the source directory. Otherwise, <c>null</c>.
    /// </param>
    internal record PluginArtifacts(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath);
}
