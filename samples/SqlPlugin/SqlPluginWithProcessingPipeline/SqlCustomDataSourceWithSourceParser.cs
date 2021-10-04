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

        public override CustomDataSourceInfo GetAboutInfo()
        {
            return new CustomDataSourceInfo
            {
                CopyrightNotice = "Copyright 2021 Microsoft Corporation. All Rights Reserved.",
                LicenseInfo = new LicenseInfo
                {
                    Name = "MIT",
                    Text = "Please see the link for the full license text.",
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/LICENSE.txt",
                },
                Owners = new[]
                {
                    new ContactInfo
                    {
                        Address = "1 Microsoft Way, Redmond, WA 98052",
                        EmailAddresses = new[]
                        {
                            "noreply@microsoft.com",
                        },
                    },
                },
                ProjectInfo = new ProjectInfo
                {
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk",
                },
            };
        }

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

            using (var reader = new StreamReader(path))
            {
                // Skip first line since namespace should be on second
                reader.ReadLine();

                var line = reader.ReadLine();

                if (line != null)
                {
                    return line.Contains(SqlPluginConstants.SqlXmlNamespace);
                }
                else
                {
                    return false;
                }
            }
        }

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            this.applicationEnvironment = applicationEnvironment;
        }
    }
}
