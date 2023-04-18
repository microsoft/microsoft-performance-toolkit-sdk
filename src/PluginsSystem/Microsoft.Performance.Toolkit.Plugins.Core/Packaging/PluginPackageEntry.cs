// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging
{
    /// <summary>
    ///     Represents an entry in a plugin package.
    /// </summary>
    public abstract class PluginPackageEntry
    {
        /// <summary>
        ///     Gets the path of the file relative to the root of the plugin package.
        /// </summary>
        public abstract string RawPath { get; }

        /// <summary>
        ///     Gets the path of the file relative to the plugin content folder if it is a plugin content file.
        ///     Otherwise, <c>null</c>.
        /// </summary>
        public abstract string ContentRelativePath { get; }

        /// <summary>
        ///    Gets a value indicating whether the entry is a metadata file.
        /// </summary>
        public abstract bool IsMetadataFile { get; }

        /// <summary>
        ///    Gets a value indicating whether the entry is a plugin file.
        /// </summary>
        public abstract bool IsPluginContentFile { get; }

        /// <summary>
        ///     Opens the entry from the plugin package.
        /// </summary>
        /// <returns>
        ///     The stream that represents the contents of the entry.
        /// </returns>
        public abstract Stream Open();
    }
}
