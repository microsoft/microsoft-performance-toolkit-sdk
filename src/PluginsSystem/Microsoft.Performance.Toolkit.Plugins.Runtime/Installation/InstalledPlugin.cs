// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

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
        /// <param name="info">
        ///     The information of the installed plugin.
        /// </param>
        /// <param name="contentsMetadata">
        ///     The contents metadata of the installed plugin.
        /// </param>
        public InstalledPlugin(
            InstalledPluginInfo info,
            PluginContentsMetadata contentsMetadata)
        {
            this.Info = info;
            this.ContentsMetadata = contentsMetadata;
        }

        /// <summary>
        ///     Gets the information of the installed plugin.
        /// </summary>
        public InstalledPluginInfo Info { get; }

        /// <summary>
        ///     Gets the contents metatdata of the installed plugin.
        /// </summary>
        public PluginContentsMetadata ContentsMetadata { get; }
    }
}
