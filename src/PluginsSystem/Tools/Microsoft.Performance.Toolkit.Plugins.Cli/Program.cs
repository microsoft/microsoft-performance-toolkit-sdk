// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    /// <summary>
    ///     Represents the entry point for the CLI.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        ///     Main entry point.
        /// </summary>
        /// <param name="args">
        ///     The command line arguments.
        /// </param>
        /// <returns>
        ///     A task whose result is the exit code. 0 on success; otherwise, non-zero.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);
            Func<Type, ILogger> loggerFactory = ConsoleLogger.Create;
            var metadataGenerator = new MetadataGenerator(loggerFactory);
            var sourceFilesValidator = new PluginSourceFilesValidator(loggerFactory);
            string schemaFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Constants.ManifestSchemaFilePath);

            string manifestSchema = File.ReadAllText(schemaFilePath);

            var manifestValidator = new PluginManifestJsonValidator(manifestSchema, loggerFactory);

            return result.MapResult(
                (PackOptions opts) => opts.Run(
                    loggerFactory,
                    sourceFilesValidator,
                    manifestValidator,
                    metadataGenerator),
                (MetadataGenOptions opts) => opts.Run(
                    loggerFactory,
                    sourceFilesValidator,
                    manifestValidator,
                    metadataGenerator),
                errs => HandleParseError(errs));
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine(errs);

            return 1;
        }
    }
}

