// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal interface IManifestLocator
    {
        bool TryLocate([NotNullWhen(true)] out string? manifestFilePath);
    }
}
