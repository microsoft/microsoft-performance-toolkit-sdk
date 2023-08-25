// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal sealed class Pack
        : IPack
    {
        private readonly IPluginContentsProcessor sourceProcessor;
        private readonly IPackageBuilder packageBuilder;
        private readonly ILogger<Pack> logger;

        public Pack(
            IPluginContentsProcessor sourceProcessor,
            IPackageBuilder packageBuilder,
            ILogger<Pack> logger)
        {
            this.sourceProcessor = sourceProcessor;
            this.packageBuilder = packageBuilder;
            this.logger = logger;
        }

        public int Run(PackOptions packOptions)
        {
            packOptions.Validate();

            ProcessedPluginContents processedSource = this.sourceProcessor.Process(packOptions);


            string? destFilePath = packOptions.OutputFileFullPath;
            if (destFilePath == null || !packOptions.Overwrite)
            {
                string destFileName = destFilePath ??
                    Path.Combine(Environment.CurrentDirectory, $"{processedSource.Metadata.Identity}{PackageConstants.PluginPackageExtension}");

                destFilePath = Program.GetValidDestFileName(destFileName);
            }

            string tmpFile = Path.GetTempFileName();
            try
            {
                this.packageBuilder.Build(processedSource, tmpFile);
                File.Move(tmpFile, destFilePath, packOptions.Overwrite);

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
