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
        public static readonly string PluginMetadataFileName = "pluginspec.json";
        public static readonly string PluginContentPath = "plugin/";

        private readonly ZipArchiveEntry zipEntry;

        internal ZipPluginPackageEntry(ZipArchiveEntry zipEntry)
        {
            this.zipEntry = zipEntry;
        }

        public override string RawPath
        {
            get
            {
                return this.zipEntry.FullName;
            }
        }

        public override bool IsMetadataFile
        {
            get
            {
                return this.RawPath.Equals(PluginMetadataFileName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override bool IsPluginContentFile
        {
            get
            {
                return this.RawPath.StartsWith(PluginContentPath, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override string RelativePath
        {
            get
            {
                if (this.IsPluginContentFile)
                {
                    return this.RawPath.Substring(PluginContentPath.Length);
                }
                else
                {
                    return this.RawPath;
                }
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
