// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Loading
{
    /// <summary>
    ///     Represents a loader that can load a plugin from an <see cref="InstalledPlugin"/>.
    /// </summary>
    public interface IInstalledPluginLoader
    {
        /// <summary>
        ///     Loads the plugin from the given <see cref="InstalledPlugin"/>.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The plugin to load.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin was loaded successfully; <c>false</c> otherwise.
        /// </returns>
        Task<bool> LoadPluginAsync(InstalledPlugin installedPlugin);
    }
}
