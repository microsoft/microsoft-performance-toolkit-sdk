// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Result
{
    public class InstalledPluginsResults
    {
        public InstalledPluginsResults(
                        IReadOnlyCollection<InstalledPlugin> validPlugins,
            IReadOnlyCollection<InstalledPluginInfo> invalidPluginsInfo)
        {
            this.ValidPlugins = validPlugins;
            this.InvalidPluginsInfo = invalidPluginsInfo;
        }

        public IReadOnlyCollection<InstalledPluginInfo> InvalidPluginsInfo { get; }

        public IReadOnlyCollection<InstalledPlugin> ValidPlugins { get; }
    }
}
