// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console
{
    internal class PluginsCli
    {
        private readonly ICommand<PackArgs> packCommand;
        private readonly ICommand<MetadataGenArgs> metadataGenCommand;
        private readonly IOptionsValidator<PackOptions, PackArgs> packOptionsValidator;
        private readonly IOptionsValidator<MetadataGenOptions, MetadataGenArgs> metadataGenOptionsValidator;
        private readonly ILogger<PluginsCli> logger;

        public PluginsCli(
            ICommand<PackArgs> packCommand,
            ICommand<MetadataGenArgs> metadataGenCommand,
            IOptionsValidator<PackOptions, PackArgs> packOptionsValidator,
            IOptionsValidator<MetadataGenOptions, MetadataGenArgs> metadataGenOptionsValidator,
            ILogger<PluginsCli> logger)
        {
            this.packCommand = packCommand;
            this.metadataGenCommand = metadataGenCommand;
            this.packOptionsValidator = packOptionsValidator;
            this.metadataGenOptionsValidator = metadataGenOptionsValidator;
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
                return 0;
            }
        }
        
        private int RunPack(PackOptions packOptions)
        {
            if (!this.packOptionsValidator.TryValidate(packOptions, out PackArgs? packArgs))
            {
                this.logger.LogError("Failed to validate pack options.");
                return 1;
            }

            return this.packCommand.Run(packArgs!);
        }

        private int RunMetadataGen(MetadataGenOptions metadataGenOptions)
        {
            if (!this.metadataGenOptionsValidator.TryValidate(metadataGenOptions, out MetadataGenArgs? metadataGenArgs))
            {
                this.logger.LogError("Failed to validate metadata gen options.");
                return 1;
            }
            
            return this.metadataGenCommand.Run(metadataGenArgs!);
        }


        private int HandleParseError(IEnumerable<Error> errs)
        {
            string errors = string.Join(Environment.NewLine, errs.Select(x => x.ToString()));
            this.logger.LogError($"Failed to parse command line arguments: \n{errors}");

            return 1;
        }
    }
}
