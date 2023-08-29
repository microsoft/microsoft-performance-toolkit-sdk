// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal sealed class PackCommand
        : ICommand<PackArgs>
    {
        private readonly IManifestLocatorFactory manifestLocatorFactory;
        private readonly IPluginArtifactsProcessor sourceProcessor;
        private readonly IPackageBuilder packageBuilder;
        private readonly ILogger<PackCommand> logger;

        public PackCommand(
            IManifestLocatorFactory manifestLocatorFactory,
            IPluginArtifactsProcessor sourceProcessor,
            IPackageBuilder packageBuilder,
            ILogger<PackCommand> logger)
        {
            this.manifestLocatorFactory = manifestLocatorFactory;
            this.sourceProcessor = sourceProcessor;
            this.packageBuilder = packageBuilder;
            this.logger = logger;
        }

        public int Run(PackArgs args)
        {
            IManifestLocator manifestLocator = this.manifestLocatorFactory.Create(args);
            if (!manifestLocator.TryLocate(out string? manifestFilePath))
            {
                this.logger.LogError("Failed to locate manifest file.");
                return 1;
            }
            
            var artifacts = new PluginArtifacts(args.SourceDirectoryFullPath, manifestFilePath);
            if (!this.sourceProcessor.TryProcess(artifacts, out ProcessedPluginResult? processedSource))
            {
                this.logger.LogError("Failed to process plugin artifacts.");
                return 1;
            }

            string? destFilePath = args.OutputFileFullPath;
            if (destFilePath == null || !args.Overwrite)
            {
                string destFileName = destFilePath ??
                    Path.Combine(Environment.CurrentDirectory, $"{processedSource!.Metadata.Identity}{PackageConstants.PluginPackageExtension}");

                destFilePath = Utils.GetAlterDestFilePath(destFileName);
            }

            string tmpFile = Path.GetTempFileName();
            try
            {
                this.packageBuilder.Build(processedSource!, tmpFile);
                File.Move(tmpFile, destFilePath, args.Overwrite);
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
