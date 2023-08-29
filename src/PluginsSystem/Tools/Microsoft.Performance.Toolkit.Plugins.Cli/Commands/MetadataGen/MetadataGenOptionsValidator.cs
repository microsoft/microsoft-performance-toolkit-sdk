// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands.MetadataGen
{
    internal class MetadataGenOptionsValidator
        : PackGenCommonOptionsValidator<MetadataGenOptions, MetadataGenArgs>
    {
        public MetadataGenOptionsValidator(ILogger<MetadataGenOptionsValidator> logger)
            : base(logger)
        {
        }

        public override bool TryValidate(MetadataGenOptions cliOptions, out MetadataGenArgs? validatedAppArgs)
        {
            validatedAppArgs = null;
            if (!TryValidateCommonOptions(cliOptions, out PackGenCommonArgs validatedCommonArgs))
            {
                this.logger.LogError("Failed to validate common options.");
                return false;
            }

            if (cliOptions.OutputDirectory == null && cliOptions.Overwrite)
            {
                this.logger.LogError("Cannot overwrite output directory when output directory is not specified.");
                return false;
            }

            string? outputDirectoryFullPath = null;
            if (cliOptions.OutputDirectory != null)
            {
                if (!Directory.Exists(cliOptions.OutputDirectory))
                {
                    this.logger.LogError($"Output directory '{cliOptions.OutputDirectory}' does not exist." +
                        $"Please create it or omit the --output option to use the current directory.");
                    return false;
                }

                outputDirectoryFullPath = Path.GetFullPath(cliOptions.OutputDirectory);
            }

            validatedAppArgs = new MetadataGenArgs(validatedCommonArgs, outputDirectoryFullPath);
            return true;
        }
    }
}
