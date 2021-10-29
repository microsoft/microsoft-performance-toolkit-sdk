// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class InvalidInternalTable
    {
        // This table requires Composite1Cooker, which requires two different source parsers: Source123 & Source4.
        // Because it spans source parsers, it isn't possible for it to retrieve all data it requires as an internal
        // table.
        //

        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{05DA37CB-F0E1-4C6A-AAC0-12EC45B71CCA}"),
            "Invalid Internal Table",
            "Consumes cookers from the given source parser.",
            "Engine",
            isInternalTable: true,
            requiredDataCookers: new[] { Composite1Cooker.DataCookerPath });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            throw new NotImplementedException();
        }
    }
}
