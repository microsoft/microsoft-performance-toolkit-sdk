// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Creates <see cref="IManifestLocator"/> instances.
    /// </summary>
    internal interface IManifestLocatorFactory
    {
        /// <summary>
        ///     Creates a <see cref="IManifestLocator"/> instance.
        /// </summary>
        /// <param name="args">
        ///     The <see cref="PackGenCommonArgs"/> to use when creating the <see cref="IManifestLocator"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="IManifestLocator"/> instance.
        /// </returns>
        IManifestLocator Create(PackGenCommonArgs args);
    }
}
