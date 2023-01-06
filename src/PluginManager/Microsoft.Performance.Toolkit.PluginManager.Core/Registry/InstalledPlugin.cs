// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Registry
{
    // TODO:
    public sealed class InstalledPlugin : IPluginInfo
    {
        public PluginIdentity Identity { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public Uri SourceUri { get; }
    }
}
