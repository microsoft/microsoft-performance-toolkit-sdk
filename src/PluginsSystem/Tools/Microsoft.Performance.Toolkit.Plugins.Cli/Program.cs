// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);
            Func<Type, ILogger> loggerFactory = ConsoleLogger.Create;

            return result.MapResult(
                (PackOptions opts) => opts.Run(),
                (MetadataGenOptions opts) => opts.Run(
                    loggerFactory,
                    new PluginSourceFilesValidator(loggerFactory),
                    new PluginManifestValidator(),
                    new MetadataGenerator()),
                errs => HandleParseError(errs));
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine(errs);

            return 1;
        }
    }
}

