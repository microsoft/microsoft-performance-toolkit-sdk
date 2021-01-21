// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlPlugin
{
    [CustomDataSource("7309FAED-6A34-4FD1-8551-7AEB5006C71E",
                      "SQL Trace Data Source",
                      "Processes SQL trace files exported as XML.")]
    [FileDataSource(".xml", "XML files exported from TRC files")]
    public class SqlCustomDataSource
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
            return new SqlCustomDataProcessor(filePath,
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
