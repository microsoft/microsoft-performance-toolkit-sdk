using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleAddIn
{
    internal class CustomDataProcessorWithSourceParser
         : CustomDataProcessorWithSourceParser<WordEvent, WordSourceParser, string>
    {
        public CustomDataProcessorWithSourceParser(ISourceParser<WordEvent, WordSourceParser, string> sourceParser, ProcessorOptions options, IApplicationEnvironment applicationEnvironment, IProcessorEnvironment processorEnvironment)
            : base(sourceParser, options, applicationEnvironment, processorEnvironment)
        {
        }
    }
}
