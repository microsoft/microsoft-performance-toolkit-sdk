// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;

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
                return this.zipEntry.FullName.Replace('\\', '/');
            }
        }

        /// <inheritdoc/>
        public override string ContentRelativePath
        {
            get
            {
                return this.EntryType == PluginPackageEntryType.ContentFile ? this.RawPath.Substring(PackageConstants.PluginContentFolderName.Length) : null;
            }
        }

        public override PluginPackageEntryType EntryType
        {
            get
            {
                if (this.RawPath.Equals(PackageConstants.PluginInfoFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return PluginPackageEntryType.InfoJsonFile;
                }
                else if (this.RawPath.Equals(PackageConstants.PluginContentsFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return PluginPackageEntryType.ContentsJsonFile;
                }
                else if (this.RawPath.StartsWith(PackageConstants.PluginContentFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    return PluginPackageEntryType.ContentFile;
                }
                else
                {
                    return PluginPackageEntryType.Unknown;
                }
            }
        }

        /// <inheritdoc/>
        public override long InstalledSize
        {
            get
            {
                return this.zipEntry.Length;
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
