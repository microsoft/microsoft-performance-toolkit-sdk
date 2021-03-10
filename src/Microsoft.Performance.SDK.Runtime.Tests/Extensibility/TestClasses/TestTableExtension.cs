// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    [Table]
    public class TestTableExtension
    {
        public static readonly TableDescriptor TableDescriptor = Any.TableDescriptor();

        public static void BuildTable(ITableBuilder builder, IDataExtensionRetrieval retrieval)
        {
        }
    }
}
