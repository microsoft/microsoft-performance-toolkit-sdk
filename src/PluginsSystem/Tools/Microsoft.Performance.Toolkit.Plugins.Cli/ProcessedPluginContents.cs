// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public record ProcessedPluginContents(
        string SourceDirectory,
        IReadOnlyList<string> ContentFiles,
        PluginMetadata Metadata,
        PluginContentsMetadata ContentsMetadata);
}
