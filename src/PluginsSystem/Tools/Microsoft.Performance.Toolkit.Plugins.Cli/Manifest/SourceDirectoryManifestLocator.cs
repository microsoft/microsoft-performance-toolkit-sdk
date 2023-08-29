// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Locates the manifest file in the source directory.
    /// </summary>
    internal class SourceDirectoryManifestLocator
        : IManifestLocator
    {
        private readonly string sourceDir;
        private readonly ILogger<SourceDirectoryManifestLocator> logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SourceDirectoryManifestLocator"/>
        /// </summary>
        /// <param name="sourceDirectory">
        ///     The source directory to search for the manifest file.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        public SourceDirectoryManifestLocator(
            string sourceDirectory,
            ILogger<SourceDirectoryManifestLocator> logger)
        {
            this.sourceDir = sourceDirectory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool TryLocate([NotNullWhen(true)] out string? manifestFilePath)
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
