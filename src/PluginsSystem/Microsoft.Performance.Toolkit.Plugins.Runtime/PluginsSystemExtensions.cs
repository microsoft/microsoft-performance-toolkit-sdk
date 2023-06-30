// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Progress;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
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
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the installation.
        /// </param>
        /// <param name="logger">
        ///     Logs messages during installation.
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

            var aggregator = new PercentageProgressAggregator(progress);

            var getStreamProgress = aggregator.CreateChild(0);

            // We want to "weight" installing more heavily, since that will probably take longer.
            // I.e., after fetching is done the aggregated progress should show less than 50%.
            var installProgress0 = aggregator.CreateChild(0);
            var installProgress1 = aggregator.CreateChild(0);

            Stream stream;
            try
            {
                stream = await availablePlugin.GetPluginPackageStream(cancellationToken, getStreamProgress);
            }
            catch (OperationCanceledException)
            {
                logger.Info("Request to fetch plugin package is cancelled.");
                throw;
            }
            catch (Exception e)
            {
                string errorMsg = $"Fails to fetch plugin {availablePlugin.Info.Metadata.Identity} " +
                    $"from {availablePlugin.Info.PackageUri}";

                logger.Error(e, errorMsg);

                throw new PluginFetchingException(errorMsg, availablePlugin.Info, e);
            }

            getStreamProgress.Report(100);

            using (stream)
            {
                try
                {
                    return await pluginsSystem.Installer.InstallPluginAsync(
                        stream,
                        availablePlugin.Info.PackageUri,
                        cancellationToken,
                        new ProgressRepeater<int>(installProgress0, installProgress1));
                }
                catch (Exception e)
                {
                    string errorMsg = $"Fails to install plugin {availablePlugin.Info.Metadata.Identity} " +
                                      $"from {availablePlugin.Info.PackageUri}";

                    logger.Error(e, errorMsg);
                    throw;
                }
                finally
                {
                    aggregator.Finish(100);
                }
            }
        }
    }
}
