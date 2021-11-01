// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

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
}
