// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.IO;
using System;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;
using Microsoft.Performance.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    /// TODO: #236
    public sealed class PluginPackage : IDisposable
    {
        private readonly ZipArchive zip;
        private static readonly string pluginContentPath = "plugin/";
        private static readonly string pluginMetadataFileName = "pluginspec.json";
        private bool disposedValue;

        public PluginPackage(string fileName)
            : this(OpenFile(fileName))
        {
        }

        public PluginPackage(Stream stream) : this(stream, false)
        {
        }

        public PluginPackage(Stream stream, bool leaveOpen)
        {
            Guard.NotNull(stream, nameof(stream));

            try
            {
                this.zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
                this.Entries = this.zip.Entries.Select(e => new PluginPackageEntry(e)).ToList().AsReadOnly();
                this.PluginMetadata = ReadMetadata();

            }
            catch when (!leaveOpen)
            {
                stream.Dispose();
                throw;
            }
        }

        public static string PluginContentPath
        {
            get
            {
                return pluginContentPath;
            }
        }

        public static string PluginMetadataFileName
        {
            get
            {
                return pluginMetadataFileName;
            }
        }

        public string Id
        {
            get
            {
                return this.PluginMetadata.Id;
            }
        }

        public Version Version
        {
            get
            {
                return this.PluginMetadata.Version;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.PluginMetadata.DisplayName;
            }
        }

        public string Description
        {
            get
            {
                return this.PluginMetadata.Description;
            }
        }

        public IReadOnlyCollection<PluginPackageEntry> Entries { get; }

        public PluginMetadata PluginMetadata { get; }

        public Task<bool> ExtractPackageAsync(
            string extractPath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(extractPath, nameof(extractPath));

            return ExtractEntriesAsync(extractPath, entry => true, cancellationToken, progress);
        }

        public Task<bool> ExtractEntriesAsync(
            string extractPath,
            Func<PluginPackageEntry, bool> predicate,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(extractPath, nameof(extractPath));

            return ExtractEntriesInternalAsync(
                this.Entries.Where(e => predicate(e)),
                extractPath,
                cancellationToken,
                progress);
        }

        private static async Task<bool> ExtractEntriesInternalAsync(
           IEnumerable<PluginPackageEntry> entries,
           string extractPath,
           CancellationToken cancellationToken,
           IProgress<int> progress)
        {
            extractPath = Path.GetFullPath(extractPath);
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                extractPath += Path.DirectorySeparatorChar;
            }

            // TODO: #238 Error handling
            foreach (PluginPackageEntry entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destPath = Path.GetFullPath(Path.Combine(extractPath, entry.RelativePath));
                if (entry.IsDirectory )
                {
                    Directory.CreateDirectory(destPath);                    
                }
                else
                {
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    using (Stream entryStream = entry.Open())
                    using (var destStream = new FileStream(
                        destPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        4096,
                        FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        await entryStream.CopyToAsync(destStream, 81920, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return true;
        }

        private static Stream OpenFile(string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private PluginMetadata ReadMetadata()
        {
            ZipArchiveEntry entry = this.zip.GetEntry(pluginMetadataFileName);
            if (entry == null)
            {
                throw new InvalidDataException($"{pluginMetadataFileName} not found in package.");
            }

            using (Stream stream = entry.Open())
            {
                if (!PluginMetadata.TryParse(stream, out PluginMetadata metadata))
                {
                    throw new InvalidDataException($"Failed to read metadata from {pluginMetadataFileName}");
                }

                return metadata;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.zip.Dispose();
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
