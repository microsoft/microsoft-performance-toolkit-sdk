// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a file or directory contained in a <see cref="PluginPackage"/>.
    /// </summary>
    public sealed class PluginPackageEntry
    {
        private readonly ZipArchiveEntry zipEntry;

        internal PluginPackageEntry(ZipArchiveEntry zipEntry)
        {
            this.zipEntry = zipEntry;
        }

        /// <summary>
        ///     Gets the relative path of this entry to the package.
        /// </summary>
        public string RelativePath
        {
            get
            {
                return this.zipEntry.FullName.Replace('\\', '/');
            }
        }

        /// <summary>
        ///     Gets whether this entry is a directory.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return this.RelativePath?.EndsWith("/") == true;
            }
        }

        /// <summary>
        ///     Opens the entry from the plugin package.
        /// </summary>
        /// <returns>
        ///     The stream that represents the contents of the entry.
        /// </returns>
        public Stream Open()
        {
            return this.zipEntry.Open();
        }
    }
}
