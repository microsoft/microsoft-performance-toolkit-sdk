﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Validates the checksum of the installed plugin directory.
    /// </summary>
    public sealed class InstalledPluginDirectoryChecksumValidator
        : IInstalledPluginValidator
    {
        private readonly IPluginsStorageDirectory pluginsStorageDirectory;
        private readonly IDirectoryChecksumCalculator checksumCalculator;

        /// <summary>
        ///     Creates an instance of the <see cref="InstalledPluginDirectoryChecksumValidator"/>.
        /// </summary>
        public InstalledPluginDirectoryChecksumValidator(
            IPluginsStorageDirectory pluginsStorageDirectory,
            IDirectoryChecksumCalculator checksumCalculator)
        {
            Guard.NotNull(pluginsStorageDirectory, nameof(pluginsStorageDirectory));
            Guard.NotNull(checksumCalculator, nameof(checksumCalculator));

            this.pluginsStorageDirectory = pluginsStorageDirectory;
            this.checksumCalculator = checksumCalculator;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateInstalledPluginAsync(InstalledPluginInfo installedPluginInfo)
        {
            Guard.NotNull(installedPluginInfo, nameof(installedPluginInfo));

            string installDirectory = this.pluginsStorageDirectory.GetPluginRootDirectory(installedPluginInfo.Identity);

            if (!Directory.Exists(installDirectory))
            {
                return false;
            }

            string checksum = await this.checksumCalculator.GetDirectoryChecksumAsync(installDirectory);

            return checksum.Equals(installedPluginInfo.Checksum);
        }
    }
}
