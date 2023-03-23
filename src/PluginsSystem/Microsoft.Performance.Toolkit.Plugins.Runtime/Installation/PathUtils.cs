// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///    Contains utility methods for working with plugin installation paths.
    /// </summary>
    internal static class PathUtils
    {
        /// <summary>
        ///     Gets the path to the directory where the plugin is installed.
        /// </summary>
        /// <param name="installationRoot">
        ///     The root directory where plugins are installed.
        /// </param>
        /// <param name="pluginIdentity">
        ///     The identity of the plugin.
        /// </param>
        /// <returns></returns>
        public static string GetPluginInstallDirectory(string installationRoot, PluginIdentity pluginIdentity)
        {
            return Path.GetFullPath(Path.Combine(installationRoot, $"{pluginIdentity.Id}-{pluginIdentity.Version}"));
        }
    }
}
