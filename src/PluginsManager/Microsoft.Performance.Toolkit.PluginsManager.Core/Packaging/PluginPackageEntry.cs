// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging
{
    public sealed class PluginPackageEntry
    {
        private readonly ZipArchiveEntry zipEntry;

        internal PluginPackageEntry(ZipArchiveEntry zipEntry)
        {
            this.zipEntry = zipEntry;
        }

        public string RelativePath
        {
            get
            {
                return this.zipEntry.FullName.Replace('\\', '/');
            }
        }

        public bool IsDirectory
        {
            get
            {
                return this.RelativePath?.EndsWith("/") == true;
            }
        }

        public Stream Open()
        {
            return this.zipEntry.Open();
        }
    }
}
