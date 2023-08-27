// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.ContentsProcessing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal class MetadataGenCommand
        : ICommand<MetadataGenArgs>
    {
        private readonly IPluginContentsProcessor sourceProcessor;
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly ISerializer<PluginContentsMetadata> contentsMetadataSerializer;
        private readonly ILogger<MetadataGenCommand> logger;

        public MetadataGenCommand(
            IPluginContentsProcessor sourceProcessor,
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
            ProcessedPluginContents processedDir = this.sourceProcessor.Process(transformedOptions);
            PluginMetadata metadata = processedDir.Metadata;
            PluginContentsMetadata contentsMetadata = processedDir.ContentsMetadata;

            bool outputSpecified = transformedOptions.OutputDirectoryFullPath != null;
            string? outputDirectory = outputSpecified ? transformedOptions.OutputDirectoryFullPath : Environment.CurrentDirectory;

            string destMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}");
            string validDestMetadataFileName = outputSpecified && transformedOptions.Overwrite ?
                destMetadataFileName : Utils.GetValidDestFileName(destMetadataFileName);

            string destContentsMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{PackageConstants.PluginContentsMetadataFileName}");
            string validDestContentsMetadataFileName = outputSpecified && transformedOptions.Overwrite ?
                destContentsMetadataFileName : Utils.GetValidDestFileName(destContentsMetadataFileName);

            try
            {
                using (FileStream fileStream = File.Open(validDestMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    this.metadataSerializer.Serialize(fileStream, metadata);
                }
            }
            catch (IOException ex)
            {
                this.logger.LogDebug(ex, $"IO exception when writing to {validDestMetadataFileName}.");
                throw new ConsoleRuntimeException($"Failed to create plugin metadata at {validDestMetadataFileName}.", ex);
            }

            this.logger.LogInformation($"Successfully created plugin metadata at {validDestMetadataFileName}.");

            try
            {
                using (FileStream fileStream = File.Open(validDestContentsMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    this.contentsMetadataSerializer.Serialize(fileStream, contentsMetadata);
                }
            }
            catch (IOException ex)
            {
                this.logger.LogDebug(ex, $"IO exception when writing to {validDestContentsMetadataFileName}.");
                throw new ConsoleRuntimeException($"Failed to create plugin contents metadata at {validDestContentsMetadataFileName}.", ex);
            }

            this.logger.LogInformation($"Successfully created plugin contents metadata at {validDestContentsMetadataFileName}.");

            return 0;
        }
    }
}
