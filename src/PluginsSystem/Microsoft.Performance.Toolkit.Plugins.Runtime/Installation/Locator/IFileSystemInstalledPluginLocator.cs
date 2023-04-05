// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a locator that can locate installed plugins on the file system.
    /// </summary>
    public interface IFileSystemInstalledPluginLocator
    {
        /// <summary>
        ///     Gets the directory where the plugin is installed.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The directory where the plugin is installed.
        /// </returns>
        string GetPluginContentDirectory(PluginIdentity pluginIdentity);

        /// <summary>
        ///     Gets the path to the plugin metadata file.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The path to the plugin metadata file.
        /// </returns>
        string GetPluginMetadataFilePath(PluginIdentity pluginIdentity);
    }
}
