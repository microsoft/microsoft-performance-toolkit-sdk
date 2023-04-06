// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Validates the checksum of the installed plugin directory.
    /// </summary>
    public sealed class InstalledPluginDirectoryChecksumValidator
        : IInstalledPluginValidator
    {
        private readonly IFileSystemInstalledPluginLocator locator;

        /// <summary>
        ///     Creates an instance of the <see cref="InstalledPluginDirectoryChecksumValidator"/>.
        /// </summary>
        public InstalledPluginDirectoryChecksumValidator(
            IFileSystemInstalledPluginLocator locator)
        {
            Guard.NotNull(locator, nameof(locator));

            this.locator = locator;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateInstalledPluginAsync(InstalledPluginInfo installedPlugin)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            string installDirectory = this.locator.GetPluginRootDirectory(new PluginIdentity(installedPlugin.Id, installedPlugin.Version));

            if (!Directory.Exists(installDirectory))
            {
                return false;
            }

            string checksum = await HashUtils.GetDirectoryHashAsync(installDirectory);

            return checksum.Equals(installedPlugin.Checksum);
        }
    }
}
