// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     A reader for plugin manifest files.
    /// </summary>
    internal interface IManifestFileReader
    {
        /// <summary>
        ///     Attempts to read the manifest file at the given path.
        /// </summary>
        /// <param name="manifestFilePath">
        ///     The path to the manifest file.
        /// </param>
        /// <param name="pluginManifest">
        ///     The parsed manifest file, if the manifest could be parsed; <c>null</c> otherwise.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the manifest could be parsed; <c>false</c> otherwise.
        /// </returns>
        bool TryRead(string manifestFilePath, [NotNullWhen(true)] out PluginManifest? pluginManifest);
    }
}
