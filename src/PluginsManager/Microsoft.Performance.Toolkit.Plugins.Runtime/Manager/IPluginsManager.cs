// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <summary>
    ///     Contains logic for discovering, installing, uninstalling and updating plugins.
    /// </summary>
    public interface IPluginsManager
        : IPluginsInstaller
    {
        /// <summary>
        ///     Gets all plugin sources managed by this plugins manager.
        /// </summary>
        IEnumerable<PluginSource> PluginSources { get; }

        /// <summary>
        ///    Raised when an error occurs interacting with a paticular <see cref="PluginSource"/>.
        ///    Subsribe to this event to handle errors related to a particular <see cref="PluginSource"/>.
        /// </summary>
        event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;
    }
}
