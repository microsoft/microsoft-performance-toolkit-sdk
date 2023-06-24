// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public interface IPluginManifestFileValidator
    {
        bool Validate(string pluginManifestPath);
    }
}