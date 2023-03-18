// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <summary>
    ///     Extension methods for <see cref="PluginsSystem"/>.
    /// </summary>
    public static class PluginsSystemExtensions
    {
        /// <summary>
        ///     Installs the plugin with the given name.
        /// </summary>
        /// <param name="pluginsSystem">
        ///     The plugins system.
        /// </param>
        /// <param name="availablePlugin">
        ///     The available plugin to install.
        /// </param>
        /// <returns>
        ///     The installed plugin.
        /// </returns>
        public static async Task<InstalledPlugin> InstallAvailablePlugin(
            this PluginsSystem pluginsSystem,
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            ILogger logger)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            Stream stream;
            try
            {
                stream = await availablePlugin.GetPluginPackageStream(cancellationToken, progress);
            }
            catch (OperationCanceledException)
            {
                logger.Info("Request to fetch plugin package is cancelled.");
                throw;
            }
            catch (Exception e)
            {
                string errorMsg = $"Fails to fetch plugin {availablePlugin.AvailablePluginInfo.Identity} " +
                    $"from {availablePlugin.AvailablePluginInfo.PluginPackageUri}";

                logger.Error(e, errorMsg);

                throw new PluginFetchingException(errorMsg, availablePlugin.AvailablePluginInfo, e);
            }

            using (stream)
            {
                return await pluginsSystem.Installer.InstallPluginAsync(
                    stream,
                    availablePlugin.AvailablePluginInfo.PluginPackageUri,
                    cancellationToken,
                    progress);
            }
        }
    }
}
