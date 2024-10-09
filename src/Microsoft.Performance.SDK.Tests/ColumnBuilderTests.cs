// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;

[TestClass]
public class ColumnBuilderTests
{
    static ColumnConfiguration config = new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "BaseColumn"));

    static ColumnVariantIdentifier projectAsDateTime = new ColumnVariantIdentifier(Guid.NewGuid(), "Project as DateTime");
    static ColumnVariantIdentifier utc = new ColumnVariantIdentifier(Guid.NewGuid(), "UTC");
    static ColumnVariantIdentifier local = new ColumnVariantIdentifier(Guid.NewGuid(), "Local");

    static IProjection<int, Timestamp> baseProj = Projection.Constant(Timestamp.Zero);
    static IProjection<int, DateTime> utcProj = Projection.Constant(DateTime.UtcNow);
    static IProjection<int, DateTime> localProj = Projection.Constant(DateTime.Now.AddHours(1));

    [UnitTest]
    public void SingleToggleTest()
    {
        CreateTableBuilder()
            .AddColumn(config, baseProj, (column) =>
            {
                column.WithToggle(projectAsDateTime, utcProj);
            });
    }

    [UnitTest]
    public void ModesTest()
    {
        CreateTableBuilder()
            .AddColumnWithModes(utc, config, utcProj, (column) =>
            {
                column.WithMode(local, utcProj);
            });
    }

    [UnitTest]
    public void ToggleWithTwoModesTest()
    {
        CreateTableBuilder()
            .AddColumn(config, baseProj, (column) =>
            {
                column.WithToggle(projectAsDateTime, utcProj, (toggle) =>
                {
                    toggle.WithModes(utc)
                        .WithMode(local, localProj);
                });
            });
    }

    [UnitTest]
    public void NestedTogglesTest()
    {
        CreateTableBuilder()
            .AddColumn(config, baseProj, (column) =>
            {
                column.WithToggle(projectAsDateTime, utcProj, (toggle) =>
                {
                    toggle.WithModes(utc)
                        .WithMode(local, localProj)
                        .WithToggle(projectAsDateTime, utcProj, (nestedToggle) =>
                        {
                            nestedToggle.WithModes(utc)
                                .WithMode(local, localProj);
                        });
                });
            });
    }

    private ITableBuilderWithRowCount CreateTableBuilder()
    {
        return new TableBuilder().SetRowCount(1);
    }
}