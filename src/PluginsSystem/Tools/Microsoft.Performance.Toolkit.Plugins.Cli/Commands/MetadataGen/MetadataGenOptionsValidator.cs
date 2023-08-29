﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands.MetadataGen
{
    /// <summary>
    ///     Validates <see cref="MetadataGenOptions"/> and converts them to <see cref="MetadataGenArgs"/>.
    /// </summary>
    internal class MetadataGenOptionsValidator
        : PackGenCommonOptionsValidator<MetadataGenOptions, MetadataGenArgs>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MetadataGenOptionsValidator"/>
        /// </summary>
        /// <param name="logger">
        ///     Logger to use for validation.
        /// </param>
        public MetadataGenOptionsValidator(ILogger<MetadataGenOptionsValidator> logger)
            : base(logger)
        {
        }
 
        protected override bool TryValidateCore(MetadataGenOptions cliOptions, PackGenCommonArgs validatedCommonAppArgs, out MetadataGenArgs? validatedAppArgs)
        {
            validatedAppArgs = null;
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

            validatedAppArgs = new MetadataGenArgs(validatedCommonAppArgs, outputDirectoryFullPath);
            return true;
        }
    }
}
