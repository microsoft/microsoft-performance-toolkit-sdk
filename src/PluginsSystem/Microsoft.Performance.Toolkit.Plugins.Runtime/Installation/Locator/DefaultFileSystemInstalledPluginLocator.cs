// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a default implementation of <see cref="IFileSystemInstalledPluginLocator"/>.
    /// </summary>
    public sealed class DefaultInstalledPluginLocator
        : IFileSystemInstalledPluginLocator
    {
        private readonly string rootDirectory;
        private static readonly string pluginMetadataFileName = @"pluginspec.json";
        private static readonly string pluginContentFolder = @"plugin/";

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultInstalledPluginLocator"/>
        /// </summary>
        /// <param name="rootDirectory">
        ///     The root directory where plugins are installed.
        /// </param>
        public DefaultInstalledPluginLocator(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        /// <inheritdoc/>
        public string GetPluginContentDirectory(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string directory = GetInstallDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, pluginContentFolder));
        }

        /// <inheritdoc/>
        public string GetPluginMetadataFilePath(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string directory = GetInstallDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, pluginMetadataFileName));
        }

        private string GetInstallDirectory(PluginIdentity pluginIdentity)
        {
            return Path.GetFullPath(Path.Combine(this.rootDirectory, $"{pluginIdentity.Id}-{pluginIdentity.Version}"));
        }
    }
}
