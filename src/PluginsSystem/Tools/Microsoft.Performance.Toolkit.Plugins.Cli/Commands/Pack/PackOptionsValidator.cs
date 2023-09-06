// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Pack
{
    internal class PackOptionsValidator
        : PackGenCommonOptionsValidator<PackOptions, PackArgs>
    {
        public PackOptionsValidator(ILogger<PackOptionsValidator> logger)
            : base(logger)
        {
        }

        protected override bool TryValidateCore(PackOptions cliOptions, PackGenCommonArgs validatedCommonAppArgs, out PackArgs? validatedAppArgs)
        {
            validatedAppArgs = null;
            if (cliOptions.OutputFilePath == null && cliOptions.Overwrite)
            {
                this.logger.LogError("Cannot overwrite output file when output file is not specified.");
                return false;
            }

            string? outputFileFullPath = null;
            if (cliOptions.OutputFilePath != null)
            {
                if (!Path.GetExtension(cliOptions.OutputFilePath).Equals(PackageConstants.PluginPackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    this.logger.LogError($"Output file must have extension '{PackageConstants.PluginPackageExtension}'.");
                    return false;
                }

                try
                {
                    outputFileFullPath = Path.GetFullPath(cliOptions.OutputFilePath);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Unable to get full path to output file: {ex.Message}");
                    return false;
                }

                string? outputDir = Path.GetDirectoryName(outputFileFullPath);
                if (!Directory.Exists(outputDir))
                {
                    this.logger.LogError($"The directory '{outputDir}' does not exist. Please provide a valid directory path or create the directory and try again.");
                }
            }

            validatedAppArgs = new PackArgs(validatedCommonAppArgs, outputFileFullPath);
            return true;
        }
    }
}
