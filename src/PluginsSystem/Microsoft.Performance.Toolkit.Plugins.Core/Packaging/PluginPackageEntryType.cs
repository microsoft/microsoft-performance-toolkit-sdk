// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

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
        ///     The entry is for the <see cref="PluginMetadata"/>.
        /// </summary>
        MetadataJsonFile,

        /// <summary>
        ///     The entry is for the <see cref="PluginContentsMetadata"/>.
        /// </summary>
        ContentsMetadataJsonFile,

        /// <summary>
        ///     The entry is a file for the plugin itself (e.g. one of its binaries).
        /// </summary>
        ContentFile,
    }
}
