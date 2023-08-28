// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console
{
    internal class PluginsCli
    {
        private readonly ICommand<PackArgs> packCommand;
        private readonly ICommand<MetadataGenArgs> metadataGenCommand;
        private readonly ILogger<PluginsCli> logger;

        public PluginsCli(
            ICommand<PackArgs> packCommand,
            ICommand<MetadataGenArgs> metadataGenCommand,
            ILogger<PluginsCli> logger)
        {
            this.packCommand = packCommand;
            this.metadataGenCommand = metadataGenCommand;
            this.logger = logger;
        }

        public int Run(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);

            try
            {
                return result.MapResult(
                    (PackOptions opts) => RunPack(opts),
                    (MetadataGenOptions opts) => RunMetadataGen(opts),
                    errs => HandleParseError(errs));
            }
            catch (Exception ex)
            {
                this.logger.LogError("Unhandled exception occurred: {0}", ex.Message);
            }

            return 1;
        }
        private int RunPack(PackOptions packOptions)
        {
            if (!TryGetValidatedPackArgs(packOptions, out PackArgs? packArgs))
            {
                return 1;
            }

            return this.packCommand.Run(packArgs);
        }

        private int RunMetadataGen(MetadataGenOptions metadataGenOptions)
        {
            if (!TryGetValidatdMetadataGenArgs(metadataGenOptions, out MetadataGenArgs? metadataGenArgs))
            {
                return 1;
            }

            return this.metadataGenCommand.Run(metadataGenArgs);
        }


        private int HandleParseError(IEnumerable<Error> errs)
        {
            string errors = string.Join(Environment.NewLine, errs.Select(x => x.ToString()));
            this.logger.LogError($"Failed to parse command line arguments: \n{errors}");

            return 1;
        }

        private bool TryGetValidatedPackArgs(PackOptions options, out PackArgs? result)
        {
            result = null;
            if (!TryGetValidatedCommonArgs(options, out PackGenCommonArgs resultBase))
            {
                return false;
            }

            if (options.OutputFilePath == null && options.Overwrite)
            {
                this.logger.LogError("Cannot overwrite output file when output file is not specified.");
                return false;
            }

            string? outputFileFullPath = null;
            if (options.OutputFilePath != null)
            {
                if (!Path.GetExtension(options.OutputFilePath).Equals(PackageConstants.PluginPackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    this.logger.LogError($"Output file must have extension '{PackageConstants.PluginPackageExtension}'.");
                    return false;
                }

                try
                {
                    outputFileFullPath = Path.GetFullPath(options.OutputFilePath);
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Unable to get full path to output file.", ex);
                    return false;
                }

                string? outputDir = Path.GetDirectoryName(outputFileFullPath);
                if (!Directory.Exists(outputDir))
                {
                    this.logger.LogError($"The directory '{outputDir}' does not exist. Please provide a valid directory path or create the directory and try again.");
                }
            }

            result = new PackArgs(resultBase, outputFileFullPath);
            return true;
        }

        private bool TryGetValidatdMetadataGenArgs(MetadataGenOptions options, out MetadataGenArgs? result)
        {
            result = null;
            if (!TryGetValidatedCommonArgs(options, out PackGenCommonArgs resultBase))
            {
                return false;
            }

            if (options.OutputDirectory == null && options.Overwrite)
            {
                this.logger.LogError("Cannot overwrite output directory when output directory is not specified.");
                return false;
            }

            string? outputDirectoryFullPath = null;
            if (options.OutputDirectory != null)
            {
                if (!Directory.Exists(options.OutputDirectory))
                {
                    this.logger.LogError($"Output directory '{options.OutputDirectory}' does not exist." +
                        $"Please create it or omit the --output option to use the current directory.");
                    return false;
                }

                outputDirectoryFullPath = Path.GetFullPath(options.OutputDirectory);
            }

            result = new MetadataGenArgs(resultBase, outputDirectoryFullPath);
            return true;
        }


        private bool TryGetValidatedCommonArgs(PackGenCommonOptions options, out PackGenCommonArgs transformed)
        {
            transformed = null!;
            if (string.IsNullOrWhiteSpace(options.SourceDirectory))
            {
                this.logger.LogError("Source directory must be specified. Use --source <path> or -s <path>.");
                return false;
            }

            if (!Directory.Exists(options.SourceDirectory))
            {
                this.logger.LogError($"Source directory '{options.SourceDirectory}' does not exist.");
                return false;
            }

            string sourceDirectoryFullPath = Path.GetFullPath(options.SourceDirectory);

            // Validate manifest file path
            string? manifestFileFullPath = null;
            if (options.ManifestFilePath != null)
            {
                if (!File.Exists(options.ManifestFilePath))
                {
                    this.logger.LogError($"Manifest file '{options.ManifestFilePath}' does not exist.");
                    return false;
                }

                manifestFileFullPath = Path.GetFullPath(options.ManifestFilePath);
            }

            transformed = new PackGenCommonArgs(sourceDirectoryFullPath, manifestFileFullPath, options.Overwrite);
            return true;
        }
    }
}
