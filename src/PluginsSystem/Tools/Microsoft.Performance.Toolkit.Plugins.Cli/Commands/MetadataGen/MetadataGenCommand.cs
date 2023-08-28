// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal class MetadataGenCommand
        : ICommand<MetadataGenArgs>
    {
        private readonly IPluginArtifactsProcessor sourceProcessor;
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly ISerializer<PluginContentsMetadata> contentsMetadataSerializer;
        private readonly ILogger<MetadataGenCommand> logger;

        public MetadataGenCommand(
            IPluginArtifactsProcessor sourceProcessor,
            ISerializer<PluginMetadata> metadataSerializer,
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer,
            ILogger<MetadataGenCommand> logger)
        {
            this.sourceProcessor = sourceProcessor;
            this.metadataSerializer = metadataSerializer;
            this.contentsMetadataSerializer = contentsMetadataSerializer;
            this.logger = logger;
        }

        public int Run(MetadataGenArgs transformedOptions)
        {
            var args = new PluginArtifacts(transformedOptions.SourceDirectoryFullPath, transformedOptions.ManifestFileFullPath);
            if (!this.sourceProcessor.TryProcess(args, out ProcessedPluginResult? processedDir))
            {
                this.logger.LogError("Failed to process plugin artifacts.");
                return 1;
            }

            PluginMetadata metadata = processedDir!.Metadata;
            PluginContentsMetadata contentsMetadata = processedDir.ContentsMetadata;

            bool outputSpecified = transformedOptions.OutputDirectoryFullPath != null;
            string? outputDirectory = outputSpecified ? transformedOptions.OutputDirectoryFullPath : Environment.CurrentDirectory;

            string destMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}");
            string validDestMetadataFileName = outputSpecified && transformedOptions.Overwrite ?
                destMetadataFileName : Utils.GetValidDestFileName(destMetadataFileName);

            string destContentsMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginContentsMetadataFileName}");
            string validDestContentsMetadataFileName = outputSpecified && transformedOptions.Overwrite ?
                destContentsMetadataFileName : Utils.GetValidDestFileName(destContentsMetadataFileName);

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
