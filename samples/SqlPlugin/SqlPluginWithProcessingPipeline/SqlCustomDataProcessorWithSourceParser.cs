// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace SqlPluginWithProcessingPipeline
{
    /// <summary>
    ///     This class only delegates work off to the source parser, so there's no logic inside of it.
    ///     <para/>
    ///     Since our table has required data cookers, the SDK takes care of making sure it
    ///     gets built.
    /// </summary>
    public class SqlCustomDataProcessorWithSourceParser
        : CustomDataProcessorWithSourceParser<SqlEvent, SqlSourceParser, string>
    {
        internal SqlCustomDataProcessorWithSourceParser(
            ISourceParser<SqlEvent, SqlSourceParser, string> sourceParser,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment)
            : base(sourceParser, options, applicationEnvironment, processorEnvironment)
        {
        }
    }
}
