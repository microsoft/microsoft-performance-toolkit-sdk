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
            var matchedFiles = Directory.EnumerateFiles(this.sourceDir, Constants.BundledManifestName, SearchOption.AllDirectories).ToList();

            if (matchedFiles.Count == 0)
            {
                this.logger.LogError($"Directory does not contain {Constants.BundledManifestName} as expected: {this.sourceDir}.");
                return false;
            }
            else if (matchedFiles.Count > 1)
            {
                this.logger.LogError($"Directory contains multiple manifests: {string.Join(", ", matchedFiles)}. Only one manifest is allowed.");
                return false;
            }
            else
            {
                manifestFilePath = matchedFiles.First();
                return true;
            }
        }
    }
}
