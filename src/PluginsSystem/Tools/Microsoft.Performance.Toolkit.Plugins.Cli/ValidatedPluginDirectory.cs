// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class ValidatedPluginDirectory
    {
        public Version SdkVersion { get; init; }

        public string? ManifestFilePath { get; init; }

        public string FullPath { get; init; }

        public long PluginSize { get; init; }
    }
}
