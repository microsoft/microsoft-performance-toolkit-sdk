// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Validates a plugin's installation directory is not already in use by a previous installation.
    /// </summary>
    public class PreviousInstallationFolderValidator
        : IPluginValidator
    {
        private readonly ILogger logger;
        private readonly IPluginsStorageDirectory pluginsStorageDirectory;

        public PreviousInstallationFolderValidator(
            ILogger logger,
            IPluginsStorageDirectory pluginsStorageDirectory)
        {
            this.logger = logger;
            this.pluginsStorageDirectory = pluginsStorageDirectory;
        }

        public ErrorInfo[] GetValidationErrors(PluginMetadata pluginMetadata)
        {
            var dir = this.pluginsStorageDirectory.GetRootDirectory(pluginMetadata.Identity);

            if (Directory.Exists(dir))
            {
                string errorMessage =
                    $"The plugin {pluginMetadata.Identity} has already been previously installed to {dir} and has not been removed. Please remove the folder and try again.";

                this.logger.Error(errorMessage);

                return new []{ new ErrorInfo(ErrorCodes.PLUGINS_VALIDATION_PreviousInstallationFolderExists, errorMessage) };
            }

            return Array.Empty<ErrorInfo>();
        }
    }
}
