﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    /// <summary>
    ///     Command to generate plugin metadata and contents metadata.
    /// </summary>
    internal class MetadataGenCommand
        : PackGenCommonCommand<MetadataGenArgs>
    {
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly ISerializer<PluginContentsMetadata> contentsMetadataSerializer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MetadataGenCommand"/>
        /// </summary>
        /// <param name="manifestLocatorFactory">
        ///     The manifest locator factory to use to create manifest locators.
        /// </param>
        /// <param name="artifactsProcessor">
        ///     The artifacts processor to use to process plugin artifacts.
        /// </param>
        /// <param name="metadataSerializer">
        ///     The serializer to use to serialize the plugin metadata.
        /// </param>
        /// <param name="contentsMetadataSerializer">
        ///     The serializer to use to serialize the plugin contents metadata.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        public MetadataGenCommand(
            IManifestLocatorFactory manifestLocatorFactory,
            IPluginArtifactsProcessor artifactsProcessor,
            ISerializer<PluginMetadata> metadataSerializer,
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer,
            ILogger<MetadataGenCommand> logger)
            : base(manifestLocatorFactory, artifactsProcessor, logger)
        {
            this.metadataSerializer = metadataSerializer;
            this.contentsMetadataSerializer = contentsMetadataSerializer;
        }

        protected override int RunCore(MetadataGenArgs args, ProcessedPluginResult processedSource)
        {
            PluginMetadata metadata = processedSource.Metadata;
            PluginContentsMetadata contentsMetadata = processedSource.ContentsMetadata;

            bool outputSpecified = args.OutputDirectoryFullPath != null;
            string? outputDirectory = outputSpecified ? args.OutputDirectoryFullPath : Environment.CurrentDirectory;
            bool shouldOverwrite = outputSpecified && args.Overwrite;

            string destMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}");
            string validDestMetadataFileName = shouldOverwrite ?
                destMetadataFileName : Utils.GetAlterDestFilePath(destMetadataFileName);

            string destContentsMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginContentsMetadataFileName}");
            string validDestContentsMetadataFileName = shouldOverwrite ?
                destContentsMetadataFileName : Utils.GetAlterDestFilePath(destContentsMetadataFileName);

            using (FileStream fileStream = File.Open(validDestMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                this.metadataSerializer.Serialize(fileStream, metadata);
            }

            this.logger.LogInformation($"Successfully created plugin metadata at {validDestMetadataFileName}.");

            using (FileStream fileStream = File.Open(validDestContentsMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                this.contentsMetadataSerializer.Serialize(fileStream, contentsMetadata);
            }

            this.logger.LogInformation($"Successfully created plugin contents metadata at {validDestContentsMetadataFileName}.");

            return 0;
        }
    }
}
