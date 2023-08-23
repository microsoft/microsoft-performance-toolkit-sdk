// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Validation;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Metadata
{
    internal interface IMetadataGenerator
    {
        public PluginMetadata Generate(ProcessedPluginDirectory pluginDirectory, PluginManifest manifest);
    }
}
