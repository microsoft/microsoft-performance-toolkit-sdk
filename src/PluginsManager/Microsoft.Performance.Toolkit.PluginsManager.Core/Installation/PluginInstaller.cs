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

        public async Task<IEnumerable<InstalledPlugin>> GetAllInstalledPlugins()
        {
            return await this.pluginRegistry.GetInstalledPluginsAsync();
        }

        public async Task<bool> InstallPluginAsync(
            PluginPackage pluginPackage,
            string installationDir,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            InstalledPlugin installedPlugin = await InstallPluginPackageCoreAsync(
                pluginPackage,
                installationDir,
                sourceUri,
                cancellationToken,
                progress);

            await this.pluginRegistry.RegisterPluginAsync(installedPlugin, CancellationToken.None);
            return true;
        }

        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            bool success = await this.pluginRegistry.UnregisterPluginAsync(installedPlugin, cancellationToken);
            if (success)
            {
                //TODO: Remove plugin from file system if not loaded anywhere.
            }

            return true;
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

            InstalledPlugin newInstalledPlugin = await InstallPluginPackageCoreAsync(
                targetPlugin,
                installationDir,
                sourceUri,
                cancellationToken,
                progress);

            bool success = await this.pluginRegistry.UpdatePlugin(currentPlugin, newInstalledPlugin);
            if (success)
            {
                //TODO: Remove plugin from file system if not loaded anywhere.
            }

            return true;
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

            bool success = await pluginPackage.ExtractPackageAsync(installationDir, cancellationToken);

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
