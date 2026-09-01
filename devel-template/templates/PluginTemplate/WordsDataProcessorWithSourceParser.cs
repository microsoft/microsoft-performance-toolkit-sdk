// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace SimplePlugin
{
    internal class WordsDataProcessorWithSourceParser
         : CustomDataProcessorWithSourceParser<WordEvent, WordSourceParser, string>
    {
        public WordsDataProcessorWithSourceParser(
            ISourceParser<WordEvent, WordSourceParser, string> sourceParser,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment)
            : base(sourceParser, options, applicationEnvironment, processorEnvironment)
        {
        }
    }
}
