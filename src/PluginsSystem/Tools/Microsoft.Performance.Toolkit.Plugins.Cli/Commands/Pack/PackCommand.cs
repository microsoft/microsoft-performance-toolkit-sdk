// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal sealed class PackCommand
        : ICommand<PackArgs>
    {
        private readonly IPluginArtifactsProcessor sourceProcessor;
        private readonly IPackageBuilder packageBuilder;
        private readonly ILogger<PackCommand> logger;

        public PackCommand(
            IPluginArtifactsProcessor sourceProcessor,
            IPackageBuilder packageBuilder,
            ILogger<PackCommand> logger)
        {
            this.sourceProcessor = sourceProcessor;
            this.packageBuilder = packageBuilder;
            this.logger = logger;
        }

        public int Run(PackArgs transformedOptions)
        {
            var args = new PluginArtifacts(transformedOptions.SourceDirectoryFullPath, transformedOptions.ManifestFileFullPath);
            if (!this.sourceProcessor.TryProcess(args, out ProcessedPluginResult? processedSource))
            {
                this.logger.LogError("Failed to process plugin artifacts.");
                return 1;
            }

            string? destFilePath = transformedOptions.OutputFileFullPath;
            if (destFilePath == null || !transformedOptions.Overwrite)
            {
                string destFileName = destFilePath ??
                    Path.Combine(Environment.CurrentDirectory, $"{processedSource!.Metadata.Identity}{PackageConstants.PluginPackageExtension}");

                destFilePath = Utils.GetValidDestFileName(destFileName);
            }

            string tmpFile = Path.GetTempFileName();
            try
            {
                this.packageBuilder.Build(processedSource!, tmpFile);
                File.Move(tmpFile, destFilePath, transformedOptions.Overwrite);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to create plugin package at {destFilePath} due to an exception {ex.Message}");
                return 1;
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }

            this.logger.LogInformation($"Successfully created plugin package at {destFilePath}");
            return 0;
        }
    }
}
