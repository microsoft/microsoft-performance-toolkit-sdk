// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;


[TestClass]
[UnitTest]
public class RootColumnBuilderTests
{
    static ColumnConfiguration baseConfig = new(new ColumnMetadata(Guid.NewGuid(), "BaseColumn"));
    static IProjection<int, Timestamp> baseProj = Projection.Constant(Timestamp.Zero);

    static ColumnVariantIdentifier projectAsDateTime = new(Guid.NewGuid(), "Project as DateTime");

    static ColumnVariantIdentifier utc = new(Guid.NewGuid(), "UTC");
    static IProjection<int, DateTime> utcProj = Projection.Constant(DateTime.UtcNow);

    static ColumnVariantIdentifier local = new(Guid.NewGuid(), "Local");
    static IProjection<int, DateTime> localProj = Projection.Constant(DateTime.UtcNow.AddHours(2));

    static ColumnVariantIdentifier showFloat = new(Guid.NewGuid(), "Float");
    static IProjection<int, float> floatProj = Projection.Constant(1f);

    static ColumnVariantIdentifier showBool = new(Guid.NewGuid(), "Bool");
    static IProjection<int, bool> boolProj = Projection.Constant(true);

    [TestMethod]
    public void SingleToggle()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .Build();
            });

        var expected = new ToggleableColumnVariant(projectAsDateTime, null);
        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void NestedToggle()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(utc, utcProj)
                    .Build();
            });

        var expected = new ToggleableColumnVariant(
            projectAsDateTime,
            new ToggleableColumnVariant(
                utc,
                null));


        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void SingleLevelOfModes()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithModes(modesBuilder =>
                    {
                        modesBuilder
                            .WithMode(projectAsDateTime, utcProj)
                            .WithMode(utc, utcProj)
                            .Build();
                    })
                    .Build();
            });

        var expected = new ModesColumnVariant(
            new[]
            {
                new ModeColumnVariant(projectAsDateTime, null),
                new ModeColumnVariant(utc, null),
            },
            0);

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModesWithDefaultIndex()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithModes(modesBuilder =>
                    {
                        modesBuilder
                            .WithMode(projectAsDateTime, utcProj)
                            .WithMode(utc, utcProj)
                            .WithDefaultMode(utc.Id)
                            .Build();
                    })
                    .Build();
            });

        var expected = new ModesColumnVariant(
            new[]
            {
                new ModeColumnVariant(projectAsDateTime, null),
                new ModeColumnVariant(utc, null),
            },
            1);

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModeWithNestedToggles()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithModes(modesBuilder =>
                    {
                        modesBuilder
                            .WithMode(projectAsDateTime, utcProj, modeBuilder =>
                            {
                                modeBuilder
                                    .WithToggle(showFloat, floatProj)
                                    .WithToggle(showBool, boolProj)
                                    .Build();
                            })
                            .WithMode(utc, utcProj)
                            .Build();
                    })
                    .Build();
            });

        var expected = new ModesColumnVariant(
            new[]
            {
                new ModeColumnVariant(
                    projectAsDateTime,
                    new ToggleableColumnVariant(
                        showFloat,
                        new ToggleableColumnVariant(showBool, null))),
                new ModeColumnVariant(utc, null),
            },
            0);

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModeWithNestedModes()
    {
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithModes(modesBuilder =>
                    {
                        modesBuilder
                            .WithMode(projectAsDateTime, utcProj, modeBuilder =>
                            {
                                modeBuilder.WithModes(nestedModes =>
                                {
                                    nestedModes
                                        .WithMode(showFloat, floatProj)
                                        .WithMode(showBool, boolProj)
                                        .Build();
                                })
                                .Build();
                            })
                            .WithMode(utc, utcProj)
                            .Build();
                    })
                    .Build();
            });

        var expected = new ModesColumnVariant(
            new[]
            {
                new ModeColumnVariant(
                    projectAsDateTime,
                    new ModesColumnVariant(
                        new[]
                        {
                            new ModeColumnVariant(showFloat, null),
                            new ModeColumnVariant(showBool, null),
                        },
                        0)),
                new ModeColumnVariant(utc, null),
            },
            0);

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    private void AssertCorrectColumnVariants(
        IColumnVariant expectedRoot, TableBuilder builtTable)
    {
        bool success = builtTable.TryGetVariantsRoot(builtTable.Columns.First(), out IColumnVariant actualRoot);
        Assert.IsTrue(success, "No variants were registered for the column, but they were expected to be.");
        Assert.AreEqual(expectedRoot, actualRoot);
    }
}