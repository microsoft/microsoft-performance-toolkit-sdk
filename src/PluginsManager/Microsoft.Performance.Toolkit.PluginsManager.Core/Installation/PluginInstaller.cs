// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Registry;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Transport;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Installation
{
    public sealed class PluginInstaller
    {
        private readonly PluginRegistry pluginRegistry;

        public PluginInstaller(PluginRegistry pluginRegistry)
        {
            this.pluginRegistry = pluginRegistry;
        }

        public async Task<bool> InstallPluginPackage(
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
                return false;
            }

            var installedPlugin = new InstalledPlugin(
                pluginPackage.Id,
                pluginPackage.Version,
                sourceUri,
                pluginPackage.DisplayName,
                pluginPackage.Description,
                installationDir,
                DateTime.UtcNow);

            await this.pluginRegistry.RegisterPluginAsync(installedPlugin, CancellationToken.None);
            return true;
        }
    }
}
