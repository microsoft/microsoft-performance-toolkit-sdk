// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, PushOptions, MetadataGenOptions>(args);

            return result.MapResult(
                (PackOptions opts) => opts.Run(),
                (PushOptions opts) => opts.Run(),
                (MetadataGenOptions opts) => opts.Run(),
                errs => HandleParseError(errs));
        }

        static int HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("error");

            return 1;
        }
    }
}

