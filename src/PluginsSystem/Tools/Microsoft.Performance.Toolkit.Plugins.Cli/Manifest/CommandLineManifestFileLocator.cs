// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal class CommandLineManifestFileLocator
        : IManifestLocator
    {
        private readonly string manifestFilePath;

        public CommandLineManifestFileLocator(string manifestFilePath)
        {
            this.manifestFilePath = manifestFilePath;
        }

        public bool TryLocate(out string? manifestFilePath)
        {
            manifestFilePath = this.manifestFilePath;
            return true;
        }
    }
}
