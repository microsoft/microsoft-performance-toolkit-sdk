// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

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
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of the <see cref="InstalledPluginDirectoryChecksumValidator"/>.
        /// </summary>
        public InstalledPluginDirectoryChecksumValidator(
            IPluginsStorageDirectory pluginsStorageDirectory,
            IDirectoryChecksumCalculator checksumCalculator,
            ILogger logger)
        {
            Guard.NotNull(pluginsStorageDirectory, nameof(pluginsStorageDirectory));
            Guard.NotNull(checksumCalculator, nameof(checksumCalculator));

            this.pluginsStorageDirectory = pluginsStorageDirectory;
            this.checksumCalculator = checksumCalculator;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateInstalledPluginAsync(InstalledPluginInfo installedPluginInfo)
        {
            Guard.NotNull(installedPluginInfo, nameof(installedPluginInfo));

            string installDirectory = this.pluginsStorageDirectory.GetRootDirectory(installedPluginInfo.Metadata.Identity);

            if (!Directory.Exists(installDirectory))
            {
                this.logger.Error($"The install directory for {installedPluginInfo.Metadata.Identity} ({installDirectory}) does not exist.");
                return false;
            }

            string checksum = await this.checksumCalculator.GetDirectoryChecksumAsync(installDirectory);

            if (checksum.Equals(installedPluginInfo.Checksum))
            {
                return true;
            }
            else
            {
                this.logger.Error($"The current checksum for {installedPluginInfo.Metadata.Identity} does not match its recorded checksum.");
                return false;
            }
        }
    }
}
