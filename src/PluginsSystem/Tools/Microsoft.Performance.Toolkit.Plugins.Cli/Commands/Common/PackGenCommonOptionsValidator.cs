// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common
{
    internal abstract class PackGenCommonOptionsValidator<TOptions, TArgs>
       : IOptionsValidator<TOptions, TArgs>
       where TOptions : PackGenCommonOptions
       where TArgs : PackGenCommonArgs
    {
        protected readonly ILogger<PackGenCommonOptionsValidator<TOptions, TArgs>> logger;

        protected PackGenCommonOptionsValidator(ILogger<PackGenCommonOptionsValidator<TOptions, TArgs>> logger)
        {
            this.logger = logger;
        }

        public abstract bool TryValidate(TOptions cliOptions, out TArgs? validatedAppArgs);

        protected bool TryValidateCommonOptions(
            PackGenCommonOptions rawOptions,
            out PackGenCommonArgs validatedAppArgs)
        {
            validatedAppArgs = null!;
            if (string.IsNullOrWhiteSpace(rawOptions.SourceDirectory))
            {
                this.logger.LogError("Source directory must be specified. Use --source <path> or -s <path>.");
                return false;
            }

            if (!Directory.Exists(rawOptions.SourceDirectory))
            {
                this.logger.LogError($"Source directory '{rawOptions.SourceDirectory}' does not exist.");
                return false;
            }

            string sourceDirectoryFullPath = Path.GetFullPath(rawOptions.SourceDirectory);

            // Validate manifest file path
            string? manifestFileFullPath = null;
            if (rawOptions.ManifestFilePath != null)
            {
                if (!File.Exists(rawOptions.ManifestFilePath))
                {
                    this.logger.LogError($"Manifest file '{rawOptions.ManifestFilePath}' does not exist.");
                    return false;
                }

                manifestFileFullPath = Path.GetFullPath(rawOptions.ManifestFilePath);
            }

            validatedAppArgs = new PackGenCommonArgs(sourceDirectoryFullPath, manifestFileFullPath, rawOptions.Overwrite);
            return true;
        }
    }
}
