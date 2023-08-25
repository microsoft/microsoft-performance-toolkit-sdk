// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal sealed class Pack
        : IPack
    {
        private readonly IOptionsValidator<PackOptions, TransformedPackOptions> optionsValidator;
        private readonly IPluginContentsProcessor sourceProcessor;
        private readonly IPackageBuilder packageBuilder;
        private readonly ILogger<Pack> logger;

        public Pack(
            IOptionsValidator<PackOptions, TransformedPackOptions> optionsValidator,
            IPluginContentsProcessor sourceProcessor,
            IPackageBuilder packageBuilder,
            ILogger<Pack> logger)
        {
            this.optionsValidator = optionsValidator;
            this.sourceProcessor = sourceProcessor;
            this.packageBuilder = packageBuilder;
            this.logger = logger;
        }

        public int Run(PackOptions packOptions)
        {
            if (!this.optionsValidator.IsValid(packOptions, out TransformedPackOptions transformedOptions))
            {
                return -1;
            }

            ProcessedPluginContents processedSource = this.sourceProcessor.Process(transformedOptions);

            string? destFilePath = transformedOptions.OutputFileFullPath;
            if (destFilePath == null || !transformedOptions.Overwrite)
            {
                string destFileName = destFilePath ??
                    Path.Combine(Environment.CurrentDirectory, $"{processedSource.Metadata.Identity}{PackageConstants.PluginPackageExtension}");

                destFilePath = Program.GetValidDestFileName(destFileName);
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
