// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class EmptyTable
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{BA8B08F5-58F6-4F5D-9C01-BD4801918500}"),
            "Empty Test Table",
            "Used by the Engine Tests to test an empty table",
            "Engine",
            requiredDataCookers: Array.Empty<DataCookerPath>());

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            // This table will not call build
        }
    }

    [InternalTable]
    public static class InvalidInternalTable
    {
        // This table requires Composite1Cooker, which requires two different source parsers: Source123 & Source4.
        // Because it spans source parsers, it isn't possible for it to retrieve all data it requires as an internal
        // table.
        //

        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{05DA37CB-F0E1-4C6A-AAC0-12EC45B71CCA}"),
            "Valid Internal Table",
            "Consumes cookers from the given source parser.",
            "Engine",
            requiredDataCookers: new[] { Composite1Cooker.DataCookerPath });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            throw new NotImplementedException();
        }
    }

    [InternalTable]
    public static class Source5InternalTable
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{708B0564-F9BA-49FA-A87C-3B25C5C7C84C}"),
            "Source5 Internal Table",
            "Used by the Engine Tests to test an internal table",
            "Engine",
            requiredDataCookers: new[] { Composite2Cooker.DataCookerPath });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            throw new NotImplementedException();
        }
    }
}
