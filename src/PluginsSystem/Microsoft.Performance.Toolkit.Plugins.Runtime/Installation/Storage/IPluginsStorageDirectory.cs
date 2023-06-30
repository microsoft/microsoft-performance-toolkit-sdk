// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents the structure of the directory on a file system where installed plugins are stored or should be stored.
    ///     All installed plugins of a given plugins system should be stored under the same root directory.
    /// </summary>
    public interface IPluginsStorageDirectory
    {
        /// <summary>
        ///     Gets the dedicated directory where the given plugin is installed or should be installed.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The directory where the given plugin is installed or should be installed.
        /// </returns>
        string GetRootDirectory(PluginIdentity pluginIdentity);

        /// <summary>
        ///     Gets the directory where the plugin content files are located or should be located.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The directory where the plugin content files are located or should be located.
        /// </returns>
        string GetContentDirectory(PluginIdentity pluginIdentity);

        /// <summary>
        ///     Gets the path to the plugin metadata file for the given plugin.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The path to the plugin metadata file.
        /// </returns>
        string GetMetadataFilePath(PluginIdentity pluginIdentity);

        /// <summary>
        ///     Gets the path to the plugin contents metadata for the given plugin.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns>
        ///     The path to the plugin contents metadata file.
        /// </returns>
        string GetContentsMetadataFilePath(PluginIdentity pluginIdentity);

        /// <summary>
        ///     Gets the directories of all installed plugins under the root directory.
        /// </summary>
        /// <returns>
        ///     The directories of all installed plugins under the root directory.
        /// </returns>
        IEnumerable<string> GetAllRootDirectories();
    }
}
