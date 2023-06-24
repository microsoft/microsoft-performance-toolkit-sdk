// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);
            Func<Type, ILogger> loggerFactory = ConsoleLogger.Create;
            var metadataGenerator = new MetadataGenerator(loggerFactory);
            var manifestValidator = new PluginManifestValidator(loggerFactory);
            var sourceFilesValidator = new PluginSourceFilesValidator(loggerFactory);

            var schemaFilePath = @"D:\CleanRepos\microsoft-performance-toolkit-sdk\src\PluginsSystem\Schemas\PluginManifestSchema.json";
            var manifestSchema = File.ReadAllText(schemaFilePath);

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

