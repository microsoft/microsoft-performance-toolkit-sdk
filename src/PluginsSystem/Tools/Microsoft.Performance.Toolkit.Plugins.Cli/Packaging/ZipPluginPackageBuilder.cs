// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Packaging
{
    /// <summary>
    ///     Creates a plugin package from a processed plugin directory using the zip format.
    /// </summary>
    internal sealed class ZipPluginPackageBuilder
        : IPackageBuilder
    {
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly ISerializer<PluginContentsMetadata> contentsMetadataSerializer;
        private readonly ILogger<ZipPluginPackageBuilder> logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZipPluginPackageBuilder"/>.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use to serialize the plugin metadata.
        /// </param>
        /// <param name="contentsMetadataSerializer">
        ///     The serializer to use to serialize the plugin contents metadata.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        public ZipPluginPackageBuilder(
            ISerializer<PluginMetadata> serializer,
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer,
            ILogger<ZipPluginPackageBuilder> logger)
        {
            this.metadataSerializer = serializer;
            this.contentsMetadataSerializer = contentsMetadataSerializer;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Build(ProcessedPluginResult processedDir, string destFilePath)
        {
            using var stream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false);

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
    }
}
