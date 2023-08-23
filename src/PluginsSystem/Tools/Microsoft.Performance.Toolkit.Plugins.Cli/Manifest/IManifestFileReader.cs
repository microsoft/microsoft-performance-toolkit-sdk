﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public interface IManifestFileReader
    {
        PluginManifest Read(string manifestFilePath);
    }
}
