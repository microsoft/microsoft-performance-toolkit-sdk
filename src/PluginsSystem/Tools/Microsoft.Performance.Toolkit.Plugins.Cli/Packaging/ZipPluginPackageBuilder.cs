// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
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

        public void Build(ProcessedPluginContents processedDir, string destFilePath)
        {
            using var stream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false);

            try
            {
                foreach (string fileInfo in processedDir.ContentFiles)
                {
                    string fileSourcePath = Path.Combine(processedDir.SourceDirectory, fileInfo);
                    string fileTargetPath = Path.Combine(PackageConstants.PluginContentFolderName, fileInfo);

                    zip.CreateEntryFromFile(fileSourcePath, fileTargetPath, CompressionLevel.Optimal);
                }

                ZipArchiveEntry metadataEntry = zip.CreateEntry(PackageConstants.PluginMetadataFileName);
                using (Stream entryStream = metadataEntry.Open())
                {
                    this.metadataSerializer.Serialize(entryStream, processedDir.Metadata);
                }

                ZipArchiveEntry contentsMetadataEntry = zip.CreateEntry(PackageConstants.PluginContentsMetadataFileName);
                using (Stream entryStream = contentsMetadataEntry.Open())
                {
                    this.contentsMetadataSerializer.Serialize(entryStream, processedDir.ContentsMetadata);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, $"Failed to create plugin package at '{destFilePath}'.");
                throw new ConsoleRuntimeException($"Package creation failed: {ex.Message}", ex);
            }
        }
    }
}
