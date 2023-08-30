// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Locates the manifest file specified on the command line.
    /// </summary>
    internal class CommandLineManifestFileLocator
        : IManifestLocator
    {
        private readonly string manifestFilePath;

        public CommandLineManifestFileLocator(string manifestFilePath)
        {
            this.manifestFilePath = manifestFilePath;
        }

        /// <inheritdoc />
        public bool TryLocate([NotNullWhen(true)] out string? manifestFilePath)
        {
            manifestFilePath = this.manifestFilePath;
            return true;
        }
    }
}
