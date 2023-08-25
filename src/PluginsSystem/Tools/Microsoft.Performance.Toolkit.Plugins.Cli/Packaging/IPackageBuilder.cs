// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    public interface IPackageBuilder
    {
        public void Build(ProcessedPluginContents sourcePath, string targetFilePath);
    }
}
