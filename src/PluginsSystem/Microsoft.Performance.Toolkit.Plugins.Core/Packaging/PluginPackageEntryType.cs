// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging
{
    /// <summary>
    ///     Represents the type of a <see cref="PluginPackageEntry"/>.
    /// </summary>
    public enum PluginPackageEntryType
    {
        /// <summary>
        ///     The entry is an unknown type.
        /// </summary>
        Unknown,

        /// <summary>
        ///     The entry is for the <see cref="PluginInfo"/>.
        /// </summary>
        InfoJsonFile,

        /// <summary>
        ///     The entry is for the <see cref="PluginContentsInfo"/>.
        /// </summary>
        ContentsInfoJsonFile,

        /// <summary>
        ///     The entry is a file for the plugin itself (e.g. one of its binaries).
        /// </summary>
        ContentFile,
    }
}
