﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public interface IManifestFileValidator
    {
        bool IsValid(string pluginManifestPath, out List<string> errorMessages);
    }
}