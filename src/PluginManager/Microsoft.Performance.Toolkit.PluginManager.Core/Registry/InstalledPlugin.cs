// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Registry
{
    public sealed class InstalledPlugin
    {
        public PluginInfo PluginInfo { get; }

        public string InstallPath { get; }

        public DateTimeOffset InstalledOn { get; }
    }
}
