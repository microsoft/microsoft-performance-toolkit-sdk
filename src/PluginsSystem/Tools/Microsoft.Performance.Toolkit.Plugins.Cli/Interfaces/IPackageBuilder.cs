// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public interface IPackageBuilder
    {
        public void Build(ValidatedPluginDirectory sourcePath, AllMetadata metadata, string targetFilePath);
    }
}
