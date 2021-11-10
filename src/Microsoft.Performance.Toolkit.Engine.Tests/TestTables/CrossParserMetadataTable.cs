// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class CrossParserMetadataTable
    {
        // This table requires Composite1Cooker, which requires two different source parsers: Source123 & Source4.
        //

        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{05DA37CB-F0E1-4C6A-AAC0-12EC45B71CCA}"),
            "Cross Parser Metadata Table",
            "Consumes cookers from multiple source parser.",
            "Engine",
            isMetadataTable: true,
            requiredDataCookers: new[] { Composite1Cooker.DataCookerPath });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            Assert.IsTrue(tableData.TryQueryOutput(
                new DataOutputPath(Source3DataCooker.DataCookerPath, "Objects"),
                out List<Source3DataObject> source3Objects));

            Assert.IsTrue(tableData.TryQueryOutput(
                new DataOutputPath(Source4DataCooker.DataCookerPath, "Objects"),
                out List<Source4DataObject> source4Objects));
        }
    }
}
