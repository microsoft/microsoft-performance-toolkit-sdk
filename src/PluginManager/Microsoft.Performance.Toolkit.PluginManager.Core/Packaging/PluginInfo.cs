// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging
{
    // For local plugin, generate on installation
    // For remote plugin, genereate on uploading to host.
    public class PluginInfo : IPluginInfo
    {
        public PluginIdentity Identity { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public Uri SourceUri { get; }
    }
}
