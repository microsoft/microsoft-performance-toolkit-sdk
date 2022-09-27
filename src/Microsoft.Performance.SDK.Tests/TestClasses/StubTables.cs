// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests
{
    [Table]
    internal sealed class StubDataTableOne
    {
        public bool TryCreateTable(ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{F3F7B534-5DC5-40FB-93D9-07FDAC073A13}"),
            "Name0",
            "Description",
            "Category");

        public static bool BuildTableWasCalled { get; private set; }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
        {
            BuildTableWasCalled = true;
        }
    }

    [Table]
    internal sealed class StubDataTableTwo
    {
        public bool TryCreateTable(ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{677CA54E-45D2-46B1-80BE-6DBA96597435}"),
            "Name1",
            "Description",
            "Category");

        public static bool BuildTableWasCalled { get; private set; }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
        {
            BuildTableWasCalled = true;
        }
    }

    [Table]
    internal sealed class StubDataTableThree
    {
        public bool TryCreateTable(ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{96D8DD5E-C0FC-4681-85E2-CFAFD1A0803C}"),
            "Name2",
            "Description",
            "Category");

        public static bool BuildTableWasCalled { get; private set; }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
        {
            BuildTableWasCalled = true;
        }
    }

    [Table]
    internal sealed class StubMetadataTableOne
    {
        public bool TryCreateTable(ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{824C0827-40E8-4DE7-ACD2-C6614E916D86}"),
            "Metadata1",
            "Description",
            TableDescriptor.DefaultCategory,
            true);

        public static bool BuildTableWasCalled { get; private set; }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
        {
            BuildTableWasCalled = true;
        }
    }

    [Table]
    internal sealed class StubMetadataTableTwo
    {
        public bool TryCreateTable(ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{2072CA7C-79F0-4FA5-9DBD-1453D117629F}"),
            "Metadata2",
            "Description",
            TableDescriptor.DefaultCategory,
            true);

        public static bool BuildTableWasCalled { get; private set; }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval cookedData)
        {
            BuildTableWasCalled = true;
        }
    }

    [Table]
    internal sealed class StubMetadataTableNoBuildMethod
    {
        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{DCE1C3DB-0522-48A1-B0B8-C04D95FA93FC}"),
            "MetadataNoBuildMethod",
            "Description",
            TableDescriptor.DefaultCategory,
            true);
    }

    [Table]
    internal sealed class StubDataTableOneNoBuildMethod
    {
        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{025EE14C-E4CC-40C4-9367-B56BE17D3D63}"),
            "DataTableOneNoBuildMethod",
            "Description",
            TableDescriptor.DefaultCategory,
            true);
    }

    [Table]
    internal sealed class StubDataTableTwoNoBuildMethod
    {
        public static TableDescriptor TableDescriptor { get; } = new TableDescriptor(
            Guid.Parse("{4AD12D02-5733-46D4-9CEB-A66E44F58D88}"),
            "DataTableTwoNoBuildMethod",
            "Description",
            TableDescriptor.DefaultCategory,
            true);
    }
}
