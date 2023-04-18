// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Results of the reading all installed plugins operations.
    /// </summary>
    public sealed class InstalledPluginsResults
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstalledPluginsResults"/> class.
        /// </summary>
        /// <param name="validPlugins">
        ///     The plugins that were successfully read.
        /// </param>
        /// <param name="failedPluginsInfo">
        ///     The plugins that failed to be read, along with the exception that caused the failure.
        /// </param>
        public InstalledPluginsResults(
            IReadOnlyCollection<InstalledPlugin> validPlugins,
            IReadOnlyCollection<(InstalledPluginInfo, Exception)> failedPluginsInfo)
        {
            Guard.NotNull(validPlugins, nameof(validPlugins));
            Guard.NotNull(validPlugins, nameof(failedPluginsInfo));

            this.ValidPlugins = validPlugins;
            this.FailedToReadPluginsInfo = failedPluginsInfo;
        }

        /// <summary>
        ///    The plugins that were successfully read.
        /// </summary>
        public IReadOnlyCollection<InstalledPlugin> ValidPlugins { get; }

        /// <summary>
        ///     The plugins that failed to be read, along with the exception that caused the failure.
        ///     The exception can be <>null</>.
        /// </summary>
        public IReadOnlyCollection<(InstalledPluginInfo, Exception)> FailedToReadPluginsInfo { get; }
    }
}
