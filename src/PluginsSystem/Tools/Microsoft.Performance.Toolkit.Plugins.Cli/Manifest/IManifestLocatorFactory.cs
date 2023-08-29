// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal interface IManifestLocatorFactory
    {
        IManifestLocator Create(PackGenCommonArgs args);
    }
}
