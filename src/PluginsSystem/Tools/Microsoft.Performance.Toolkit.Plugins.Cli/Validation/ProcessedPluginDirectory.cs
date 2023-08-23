// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Validation
{
    public record ProcessedPluginDirectory(
        string FullPath,
        string? ManifestFilePath,
        Version SdkVersion,
        long PluginSize,
        PluginContentsMetadata ContentsMetadata);
}
