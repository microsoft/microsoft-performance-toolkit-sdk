// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    /// <summary>
    ///     Represents a file or directory contained in a <see cref="PluginPackage"/>.
    /// </summary>
    public sealed class ZipPluginPackageEntry
        : PluginPackageEntry
    {
        private readonly ZipArchiveEntry zipEntry;

        internal ZipPluginPackageEntry(ZipArchiveEntry zipEntry)
        {
            this.zipEntry = zipEntry;
        }

        /// <summary>
        ///     Gets the relative path of this entry to the package.
        /// </summary>
        public override string RelativePath
        {
            get
            {
                return this.zipEntry.FullName;
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
        public override Stream Open()
        {
            return this.zipEntry.Open();
        }
    }
}
