// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Processing
{
    /// <summary>
    ///     Represents the result of processing plugin artifacts.
    /// </summary>
    /// <param name="SourceDirectory">
    ///     The path to the directory containing the plugin binaries.
    /// </param>
    /// <param name="ContentFiles">
    ///     The relative paths of the plugin content files to the source directory.
    /// </param>
    /// <param name="Metadata">
    ///     The generated plugin metadata.
    /// </param>
    /// <param name="ContentsMetadata">
    ///     The generated plugin contents metadata.
    /// </param>
    public record ProcessedPluginResult(
        string SourceDirectory,
        IReadOnlyList<string> ContentFiles,
        PluginMetadata Metadata,
        PluginContentsMetadata ContentsMetadata);
}
