// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public interface IManifestFileReader
    {
        bool TryRead(string manifestFilePath, [NotNullWhen(true)] out PluginManifest? pluginManifest);
    }
}
