// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Locates the manifest file.
    /// </summary>
    internal interface IManifestLocator
    {
        /// <summary>
        ///     Attempts to locate the manifest file.
        /// </summary>
        /// <param name="manifestFilePath">
        ///     The path to the manifest file, if found; <c>null</c> otherwise.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the manifest file was found; <c>false</c> otherwise.
        /// </returns>
        bool TryLocate([NotNullWhen(true)] out string? manifestFilePath);
    }
}
