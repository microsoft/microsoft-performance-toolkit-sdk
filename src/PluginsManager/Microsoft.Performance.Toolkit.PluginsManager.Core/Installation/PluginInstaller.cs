// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
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

        public async Task<IReadOnlyCollection<InstalledPlugin>> GetAllInstalledPlugins(
            CancellationToken cancellationToken)
        {
            using (this.pluginRegistry.UseLock(cancellationToken))
            {
                return await this.pluginRegistry.GetInstalledPluginsAsync();
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

                return await this.pluginRegistry.RegisterPluginAsync(installedPlugin, CancellationToken.None);
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
                bool success = await this.pluginRegistry.UnregisterPluginAsync(installedPlugin, cancellationToken);
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
        ///     Checks if the plugin is still being installed in the registry.
        /// </summary>
        /// <param name="installedPlugin"></param>
        /// <returns></returns>
        public Task<bool> VerifyInstalled(InstalledPlugin installedPlugin)
        {
            return Task.FromResult(true);
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
