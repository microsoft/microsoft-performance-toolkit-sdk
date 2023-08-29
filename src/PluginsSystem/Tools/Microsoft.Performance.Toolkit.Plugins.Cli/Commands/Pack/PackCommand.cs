// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal sealed class PackCommand
        : PackGenCommonCommand<PackArgs>
    {
        private readonly IPackageBuilder packageBuilder;

        public PackCommand(
            IManifestLocatorFactory manifestLocatorFactory,
            IPluginArtifactsProcessor artifactsProcessor,
            IPackageBuilder packageBuilder,
            ILogger<PackCommand> logger)
            : base(manifestLocatorFactory, artifactsProcessor, logger)
        {
            this.packageBuilder = packageBuilder;
        }

        protected override int RunCore(PackArgs args, ProcessedPluginResult processedSource)
        {
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
