// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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

        /// <inheritdoc/>
        public override string RawPath
        {
            get
            {
                return this.zipEntry.FullName;
            }
        }

        /// <inheritdoc/>
        public override bool IsMetadataFile
        {
            get
            {
                return this.RawPath.Equals(PackageConstants.PluginMetadataFileName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <inheritdoc/>
        public override bool IsPluginContentFile
        {
            get
            {
                return this.RawPath.StartsWith(PackageConstants.PluginContentFolderName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <inheritdoc/>
        public override string ContentRelativePath
        {
            get
            {
                return this.IsPluginContentFile ? this.RawPath.Substring(PackageConstants.PluginContentFolderName.Length) : null;
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
