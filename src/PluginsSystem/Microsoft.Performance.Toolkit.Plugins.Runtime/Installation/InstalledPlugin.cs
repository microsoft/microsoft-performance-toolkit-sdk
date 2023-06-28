// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents an installed plugin.
    /// </summary>
    public sealed class InstalledPlugin
    {
        /// <summary>
        ///     Creates an instance of the <see cref="InstalledPlugin"/>.
        /// </summary>
        /// <param name="installedPluginInfo">
        ///     The information of the installed plugin.
        /// </param>
        /// <param name="pluginContentsInfo">
        ///     The contents of the installed plugin.
        /// </param>
        public InstalledPlugin(
            InstalledPluginInfo installedPluginInfo,
            PluginContentsInfo pluginContentsInfo)
        {
            this.Info = installedPluginInfo;
            this.ContentsInfo = pluginContentsInfo;
        }

        /// <summary>
        ///     Gets the information of the installed plugin.
        /// </summary>
        public InstalledPluginInfo Info { get; }

        /// <summary>
        ///     Gets the contents of the installed plugin.
        /// </summary>
        public PluginContentsInfo ContentsInfo { get; }
    }
}
