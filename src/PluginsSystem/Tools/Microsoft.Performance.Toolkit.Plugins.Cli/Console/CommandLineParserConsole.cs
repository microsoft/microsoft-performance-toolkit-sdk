// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console
{
    internal class CommandLineParserConsole
        : IConsole
    {
        private readonly ICommand<PackArgs> packCommand;
        private readonly ICommand<MetadataGenArgs> metadataGenCommand;
        private readonly ILogger<CommandLineParserConsole> logger;

        public CommandLineParserConsole(
            ICommand<PackArgs> packCommand,
            ICommand<MetadataGenArgs> metadataGenCommand,
            ILogger<CommandLineParserConsole> logger)
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
            catch (ArgumentValidationException ex)
            {
                this.logger.LogError("Invalid arguments. {0}", ex.Message);
            }
            catch (InvalidPluginContentException ex)
            {
                this.logger.LogError("Invalid plugin content. {0}", ex.Message);
            }
            catch (InvalidManifestException ex)
            {
                this.logger.LogError("Invalid manifest file. {0}", ex.Message);
            }
            catch (ConsoleRuntimeException ex)
            {
                this.logger.LogError("A runtime error occurred: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Unhandled exception occurred: {0}", ex.Message);
            }

            return 1;
        }
        private int RunPack(PackOptions packOptions)
        {
            if (!TryGetValidatedPackArgs(packOptions, out PackArgs packArgs))
            {
                return 1;
            }

            return this.packCommand.Run(packArgs);
        }

        private int RunMetadataGen(MetadataGenOptions metadataGenOptions)
        {
            if (!TryGetValidatdMetadataGenArgs(metadataGenOptions, out MetadataGenArgs metadataGenArgs))
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
            if (!IsValidCommon(options, out PackGenCommonArgs resultBase))
            {
                return false;
            }

            if (options.OutputFilePath == null && options.Overwrite)
            {
                throw new ArgumentValidationException("Cannot overwrite output file when output file is not specified.");
            }

            string? outputFileFullPath = null;
            if (options.OutputFilePath != null)
            {
                if (!Path.GetExtension(options.OutputFilePath).Equals(PackageConstants.PluginPackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentValidationException($"Output file must have extension '{PackageConstants.PluginPackageExtension}'.");
                }

                try
                {
                    outputFileFullPath = Path.GetFullPath(options.OutputFilePath);
                }
                catch (Exception ex)
                {
                    throw new ArgumentValidationException("Unable to get full path to output file.", ex);
                }

                string? outputDir = Path.GetDirectoryName(outputFileFullPath);
                if (!Directory.Exists(outputDir))
                {
                    throw new ArgumentValidationException($"The directory '{outputDir}' does not exist. Please provide a valid directory path or create the directory and try again.");
                }
            }

            result = new PackArgs(resultBase, outputFileFullPath);
            return true;
        }

        private bool TryGetValidatdMetadataGenArgs(MetadataGenOptions options, out MetadataGenArgs? result)
        {
            result = null;
            if (!IsValidCommon(options, out PackGenCommonArgs resultBase))
            {
                return false;
            }

            if (options.OutputDirectory == null && options.Overwrite)
            {
                throw new ArgumentValidationException("Cannot overwrite output directory when output directory is not specified.");
            }

            string? outputDirectoryFullPath = null;
            if (options.OutputDirectory != null)
            {
                if (!Directory.Exists(options.OutputDirectory))
                {
                    throw new ArgumentValidationException($"Output directory '{options.OutputDirectory}' does not exist. Please create it or omit the --output option to use the current directory.");
                }

                outputDirectoryFullPath = Path.GetFullPath(options.OutputDirectory);
            }

            result = new MetadataGenArgs(resultBase, outputDirectoryFullPath);
            return true;
        }


        private bool IsValidCommon(PackGenCommonOptions options, out PackGenCommonArgs transformed)
        {
            // Validate source directory
            if (string.IsNullOrWhiteSpace(options.SourceDirectory))
            {
                throw new ArgumentValidationException("Source directory must be specified. Use --source <path> or -s <path>.");
            }

            if (!Directory.Exists(options.SourceDirectory))
            {
                throw new ArgumentValidationException($"Source directory '{options.SourceDirectory}' does not exist.");
            }

            var sourceDirectoryFullPath = Path.GetFullPath(options.SourceDirectory);

            string manifestFileFullPath = null;
            // Validate manifest file path
            if (options.ManifestFilePath != null)
            {
                if (!File.Exists(options.ManifestFilePath))
                {
                    throw new ArgumentValidationException($"Manifest file '{options.ManifestFilePath}' does not exist.");
                }

                manifestFileFullPath = Path.GetFullPath(options.ManifestFilePath);
            }

            transformed = new PackGenCommonArgs(sourceDirectoryFullPath, manifestFileFullPath, options.Overwrite);

            return true;
        }
    }
}
