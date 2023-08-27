// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.ContentsProcessing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal sealed class PackCommand
        : ICommand<PackArgs>
    {
        private readonly IPluginContentsProcessor sourceProcessor;
        private readonly IPackageBuilder packageBuilder;
        private readonly ILogger<PackCommand> logger;

        public PackCommand(
            IPluginContentsProcessor sourceProcessor,
            IPackageBuilder packageBuilder,
            ILogger<PackCommand> logger)
        {
            this.sourceProcessor = sourceProcessor;
            this.packageBuilder = packageBuilder;
            this.logger = logger;
        }

        public int Run(PackArgs transformedOptions)
        {
            ProcessedPluginContents processedSource = this.sourceProcessor.Process(transformedOptions);

            string? destFilePath = transformedOptions.OutputFileFullPath;
            if (destFilePath == null || !transformedOptions.Overwrite)
            {
                string destFileName = destFilePath ??
                    Path.Combine(Environment.CurrentDirectory, $"{processedSource.Metadata.Identity}{PackageConstants.PluginPackageExtension}");

                destFilePath = Utils.GetValidDestFileName(destFileName);
            }

            string tmpFile = Path.GetTempFileName();
            try
            {
                this.packageBuilder.Build(processedSource, tmpFile);
                File.Move(tmpFile, destFilePath, transformedOptions.Overwrite);

                this.logger.LogInformation($"Successfully created plugin package at {destFilePath}");
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }

            return 0;
        }
    }
}
