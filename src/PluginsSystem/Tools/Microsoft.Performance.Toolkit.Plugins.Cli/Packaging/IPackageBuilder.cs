// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    public interface IPackageBuilder
    {
        public void Build(ProcessedPluginResult sourcePath, string targetFilePath);
    }
}
