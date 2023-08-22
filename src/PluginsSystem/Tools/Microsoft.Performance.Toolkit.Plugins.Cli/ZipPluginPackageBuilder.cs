// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public sealed class ZipPluginPackageBuilder
        : IPackageBuilder
    {
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly ISerializer<PluginContentsMetadata> contentsMetadataSerializer;
        private readonly ILogger<ZipPluginPackageBuilder> logger;

        public ZipPluginPackageBuilder(
            ISerializer<PluginMetadata> serializer,
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer,
            ILogger<ZipPluginPackageBuilder> logger)
        {
            this.metadataSerializer = serializer;
            this.contentsMetadataSerializer = contentsMetadataSerializer;
            this.logger = logger;
        }
        
        public void Build(ValidatedPluginDirectory dir, AllMetadata allMetadata, string destFilePath)
        {
            string sourcePath = dir.FullPath;
            using var stream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false);
            var dirInfo = new DirectoryInfo(sourcePath);

            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string fileSourcePath = fileInfo.FullName;
                string relPath = Path.GetRelativePath(sourcePath, fileSourcePath);
                string fileTargetPath = Path.Combine(PackageConstants.PluginContentFolderName, relPath);
                zip.CreateEntryFromFile(fileSourcePath, fileTargetPath, CompressionLevel.Fastest);
            }

            ZipArchiveEntry metadataEntry = zip.CreateEntry(PackageConstants.PluginMetadataFileName);
            using (Stream entryStream = metadataEntry.Open())
            {
                this.metadataSerializer.Serialize(entryStream, allMetadata.Metadata);
            }

            ZipArchiveEntry contentsMetadataEntry = zip.CreateEntry(PackageConstants.PluginContentsMetadataFileName);
            using (Stream entryStream = contentsMetadataEntry.Open())
            {
                this.contentsMetadataSerializer.Serialize(entryStream, allMetadata.ContentsMetadata);
            }
        }
    }
}
