// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal class SourceDirectoryManifestLocator
        : IManifestLocator
    {
        private readonly string sourceDir;
        private readonly ILogger<SourceDirectoryManifestLocator> logger;

        public SourceDirectoryManifestLocator(
            string sourceDirectory,
            ILogger<SourceDirectoryManifestLocator> logger)
        {
            this.sourceDir = sourceDirectory;
            this.logger = logger;
        }

        public bool TryLocate(out string? manifestFilePath)
        {
            manifestFilePath = null;
            foreach (string file in Directory.EnumerateFiles(this.sourceDir, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                if (fileName.Equals(Constants.BundledManifestName, StringComparison.OrdinalIgnoreCase))
                {
                    if (manifestFilePath != null)
                    {
                        this.logger.LogError($"Directory contains multiple manifests: {manifestFilePath}, {file}. Only one manifest is allowed.");
                        return false;
                    }
                    manifestFilePath = file;
                }
            }

            if (manifestFilePath == null)
            {
                this.logger.LogError($"Directory does not contain {Constants.BundledManifestName} as expected: {this.sourceDir}.");
                return false;
            }

            return true;
        }
    }
}
