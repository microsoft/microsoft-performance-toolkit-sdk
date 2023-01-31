// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Concurrency;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Registry;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Installation
{
    public sealed class PluginInstaller
    {
        private readonly PluginRegistry pluginRegistry;

        public PluginInstaller(PluginRegistry pluginRegistry)
        {
            this.pluginRegistry = pluginRegistry;
        }

        public async Task<IReadOnlyCollection<InstalledPlugin>> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken)
        {
            using (this.pluginRegistry.UseLock(cancellationToken))
            {
                return await this.pluginRegistry.GetInstalledPlugins();
            }  
        }

        public async Task<bool> InstallPluginAsync(
            PluginPackage pluginPackage,
            string installationDir,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            using (this.pluginRegistry.UseLock(cancellationToken))
            {
                InstalledPlugin installedPlugin = await InstallPluginPackageCoreAsync(
                    pluginPackage,
                    installationDir,
                    sourceUri,
                    cancellationToken,
                    progress);

                return await this.pluginRegistry.RegisterPluginAsync(installedPlugin);
            }
        }

        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            using (this.pluginRegistry.UseLock(cancellationToken))
            {
                bool success = await this.pluginRegistry.UnregisterPluginAsync(installedPlugin);
                if (success)
                {
                    //TODO: Mark these files for clean up.
                }

                return success;
            }
        }

        public async Task<bool> UpdatePluginAsync(
            InstalledPlugin currentPlugin,
            PluginPackage targetPlugin,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(currentPlugin, nameof(currentPlugin));
            Guard.NotNull(targetPlugin, nameof(targetPlugin));

            string installationDir = Directory.GetParent(currentPlugin.InstallPath)?.FullName;

            using (this.pluginRegistry.UseLock(cancellationToken))
            {
                InstalledPlugin newInstalledPlugin = await InstallPluginPackageCoreAsync(
                  targetPlugin,
                  installationDir,
                  sourceUri,
                  cancellationToken,
                  progress);

                bool success = await this.pluginRegistry.UpdatePlugin(currentPlugin, newInstalledPlugin);
                if (success)
                {
                    //TODO: Mark these files for clean up.
                }

                return success;
            }
        }

        /// <summary>
        ///     Verifies the given installed plugin matches the reocrd in the plugin registry.
        /// </summary>
        /// <param name="installedPlugin">
        ///     An installed plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the installed plugin matches the record in the plugin registry. <c>false</c> otherwise.
        /// </returns>
        public async Task<bool> VerifyInstalledAsync(InstalledPlugin installedPlugin, CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            Func<InstalledPlugin, bool> predicate = plugin => plugin.Equals(installedPlugin);
            return await CheckInstalledCoreAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Checks if any plugin with the given ID has been installed to the plugin registry.
        /// </summary>
        /// <param name="pluginId">
        ///     A plugin identifier.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin is currently installed. <c>false</c> otherwise.
        /// </returns>
        public async Task<bool> IsInstalledAsync(string pluginId, CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            Func<InstalledPlugin, bool> predicate = plugin => plugin.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase);
            return await CheckInstalledCoreAsync(predicate, cancellationToken);
        }

        private async Task<bool> CheckInstalledCoreAsync(
            Func<InstalledPlugin, bool> predicate,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(predicate, nameof(predicate));

            IReadOnlyCollection<InstalledPlugin> installedPlugins = await GetAllInstalledPluginsAsync(cancellationToken);
            InstalledPlugin matchingPlugin = installedPlugins.SingleOrDefault(p => predicate(p));

            return matchingPlugin != null;
        }

        private async Task<InstalledPlugin> InstallPluginPackageCoreAsync(
            PluginPackage pluginPackage,
            string installationDir,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackage, nameof(pluginPackage));
            Guard.NotNull(installationDir, nameof(installationDir));

            installationDir = Path.GetFullPath(Path.Combine(installationDir, $"{pluginPackage.Id}-{pluginPackage.Version}"));

            bool success = await pluginPackage.ExtractPackageAsync(installationDir, cancellationToken, progress);

            if (!success)
            {
                return null;
            }

            var installedPlugin = new InstalledPlugin(
                pluginPackage.Id,
                pluginPackage.Version,
                sourceUri,
                pluginPackage.DisplayName,
                pluginPackage.Description,
                installationDir,
                DateTime.UtcNow);

            return installedPlugin;
        }
    }
}
