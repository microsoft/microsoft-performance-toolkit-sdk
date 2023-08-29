// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    /// <summary>
    ///     Defines the interface for a package builder.
    /// </summary>
    internal interface IPackageBuilder
    {
        /// <summary>
        ///     Builds a package from the given source path and writes it to the given target file path.
        /// </summary>
        /// <param name="sourcePath">
        ///     The source path to build from.
        /// </param>
        /// <param name="targetFilePath">
        ///     The target file path to write the package to.
        /// </param>
        void Build(ProcessedPluginResult sourcePath, string targetFilePath);
    }
}
