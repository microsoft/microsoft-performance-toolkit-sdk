// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestTables
{
    [Table]
    public static class EmptyTableWithIsData
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{D5088F74-E5C2-4BB3-BB60-3FA6983EC7EF}"),
            "Empty Test Table with IsDataAvailable",
            "Used by the Engine Tests to test an empty table with IsDataAvailable",
            "Engine",
            requiredDataCookers: Array.Empty<DataCookerPath>());

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            // This table will not call build
        }

        public static bool IsDataAvailable(IDataExtensionRetrieval tableData)
        {
            return false;
        }
    }
}
