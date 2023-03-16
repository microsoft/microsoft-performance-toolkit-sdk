// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <summary>
    ///     Contains logic for discovering, installing, uninstalling and updating plugins.
    /// </summary>
    public interface IPluginsManager
        : IPluginsInstaller,
          IPluginsDiscoveryManager
    {
        Task<InstalledPluginInfo> TryInstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}
