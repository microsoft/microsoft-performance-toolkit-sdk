// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Validation;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    public interface IPackageBuilder
    {
        public void Build(ProcessedPluginDirectory sourcePath, PluginMetadata metadata, string targetFilePath);
    }
}
