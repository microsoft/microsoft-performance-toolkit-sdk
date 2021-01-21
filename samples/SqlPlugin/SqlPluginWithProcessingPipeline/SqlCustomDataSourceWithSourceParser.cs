// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlPluginWithProcessingPipeline
{
    [CustomDataSource("D075CBD0-EAB5-41CD-81FF-66FF590D5089",
                      "SQL Trace Data Source With Data Cookers",
                      "Processes SQL trace files exported as XML.")]
    [FileDataSource(".xml", "XML files exported from TRC files")]
    public class SqlCustomDataSourceWithSourceParser
        : CustomDataSourceBase
    {
        private IApplicationEnvironment applicationEnvironment;

        protected override ICustomDataProcessor CreateProcessorCore(IEnumerable<IDataSource> dataSources,
                                                                    IProcessorEnvironment processorEnvironment,
                                                                    ProcessorOptions options)
        {
            //
            // Since this is a demo, we are going to assume only one file is opened
            // at a time and just get the .First() data source passed in. For a real
            // plugin, every data source should be used.
            //

            var filePath = dataSources.First().GetUri().LocalPath;
            var parser = new SqlSourceParser(filePath);
            return new SqlCustomDataProcessorWithSourceParser(parser,
                                                              options,
                                                              this.applicationEnvironment,
                                                              processorEnvironment,
                                                              this.AllTables,
                                                              this.MetadataTables);
        }

        protected override bool IsFileSupportedCore(string path)
        {
            // Peek inside the XML and make sure our XML namespace is declared and used

            var lines = File.ReadLines(path);
            if (lines.Count() < 2)
            {
                return false;
            }

            return lines.ElementAt(1).Contains(SqlPluginConstants.SqlXmlNamespace);
        }

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            this.applicationEnvironment = applicationEnvironment;
        }
    }
}
