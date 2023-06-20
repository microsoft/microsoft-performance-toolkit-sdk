// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public sealed class PluginPackageBuilder
        : IDisposable
    {
        private readonly ZipArchive zip;
        private readonly ISerializer<PluginMetadata> metadataSerializer;

        public PluginPackageBuilder(
            string fileName,
            ISerializer<PluginMetadata> serializer)
            : this (CreateFile(fileName), serializer)
        {
        }

        public PluginPackageBuilder(
            Stream stream,
            ISerializer<PluginMetadata> serializer)
        {
            Guard.NotNull(stream, nameof(stream));
            
            try
            {
                this.zip = new ZipArchive(stream, ZipArchiveMode.Create, false);
                this.metadataSerializer = serializer;
            }
            catch
            {
                this.zip?.Dispose();
                throw;
            }
        }

        public void AddContent(
            string sourcePath,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(sourcePath, nameof(sourcePath));

            sourcePath = Path.GetFullPath(sourcePath);
            var dirInfo = new DirectoryInfo(sourcePath);

            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileSourcePath = fileInfo.FullName;
                string relPath = fileInfo.FullName.Substring(dirInfo.FullName.Length+1);
                string fileTargetPath = Path.Combine(PackageConstants.PluginContentFolderName.Replace("/","\\"), relPath);
                this.zip.CreateEntryFromFile(fileSourcePath, fileTargetPath, CompressionLevel.Fastest);
            }
        }

        public void AddMetadata(PluginMetadata metadata)
        {
            Guard.NotNull(metadata, nameof(metadata));

            ZipArchiveEntry entry = this.zip.CreateEntry(PackageConstants.PluginMetadataFileName);
            using (Stream entryStream = entry.Open())
            {
                this.metadataSerializer.Serialize(entryStream, metadata);
            }
        }

        public void Dispose()
        {
            this.zip?.Dispose();
        }

        private static Stream CreateFile(string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));
            
            return new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        }
    }
}
