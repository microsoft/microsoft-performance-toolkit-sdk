// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.ContentsProcessing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    public interface IPackageBuilder
    {
        public void Build(ProcessedPluginContents sourcePath, string targetFilePath);
    }
}
