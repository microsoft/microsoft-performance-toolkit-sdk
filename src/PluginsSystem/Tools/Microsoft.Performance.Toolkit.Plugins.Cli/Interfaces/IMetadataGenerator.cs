// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal interface IMetadataGenerator
    {
        public AllMetadata? TryGen(ValidatedPluginDirectory pluginDirectory, PluginManifest manifest);
    }
}
