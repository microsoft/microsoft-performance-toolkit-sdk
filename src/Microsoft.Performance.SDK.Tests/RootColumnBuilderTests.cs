// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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
    private static readonly ColumnConfiguration baseConfig = new(new ColumnMetadata(Guid.NewGuid(), "BaseColumn"));
    private static readonly IProjection<int, Timestamp> baseProj = Projection.Constant(Timestamp.Zero);

    private static readonly ColumnVariantIdentifier projectAsDateTime = new(Guid.NewGuid(), "Project as DateTime");

    private static readonly ColumnVariantIdentifier utc = new(baseConfig.Metadata.Guid, "UTC");
    private static readonly IProjection<int, DateTime> utcProj = Projection.Constant(DateTime.UtcNow);

    private static readonly ColumnVariantIdentifier local = new(Guid.NewGuid(), "Local");
    private static readonly IProjection<int, DateTime> localProj = Projection.Constant(DateTime.UtcNow.AddHours(2));

    private static readonly ColumnVariantIdentifier showFloat = new(Guid.NewGuid(), "Float");
    private static readonly IProjection<int, float> floatProj = Projection.Constant(1f);

    private static readonly ColumnVariantIdentifier showBool = new(Guid.NewGuid(), "Bool");
    private static readonly IProjection<int, bool> boolProj = Projection.Constant(true);

    [TestMethod]
    public void SingleToggle()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .Build();
            });

        var expected = Toggle(projectAsDateTime);
        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void NestedToggle()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(utc, utcProj)
                    .Build();
            });

        var expected = Toggle(
            projectAsDateTime,
            Toggle(utc));


        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void SingleLevelOfModes()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                builder
                    .WithModes(utc.Name)
                    .WithMode(local, localProj)
                    .Build();
            });

        var expected = Modes(
            0,
            Mode(utc),
            Mode(local));

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModesWithDefaultIndex()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                builder
                    .WithModes(utc.Name)
                    .WithMode(local, localProj)
                    .WithDefaultMode(local.Guid)
                    .Build();
            });

        var expected = Modes(
            1,
            Mode(utc),
            Mode(local));

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModeWithNestedToggles()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                builder
                    .WithModes(
                        utc.Name,
                        modeBuilder =>
                        {
                            modeBuilder
                                .WithToggle(showFloat, floatProj)
                                .WithToggle(showBool, boolProj)
                                .Build();
                        })
                    .WithMode(local, localProj)
                    .Build();
            });

        var expected = Modes(
            0,
            Mode(
                utc,
                Toggle(
                    showFloat,
                    Toggle(showBool))),
            Mode(local));
        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void ModeWithNestedModes()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                builder
                    .WithModes(utc.Name)
                    .WithMode(local, floatProj, modeBuilder =>
                    {
                        modeBuilder.WithToggledModes(
                                "Foo",
                                nestedModes =>
                                {
                                    nestedModes
                                        .WithMode(showFloat, floatProj)
                                        .WithMode(showBool, boolProj)
                                        .Build();
                                })
                            .Build();
                    })
                    .Build();
            });

        var expected = Modes(
            0,
            Mode(utc),
            Mode(local,
                ModesToggle(
                    "Foo",
                    Modes(
                        0,
                        Mode(showFloat),
                        Mode(showBool)))));

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    private void AssertCorrectColumnVariants(
        IColumnVariant expectedRoot, TableBuilder builtTable)
    {
        var success = builtTable.TryGetVariantsRoot(builtTable.Columns.First(), out var actualRoot);
        Assert.IsTrue(success, "No variants were registered for the column, but they were expected to be.");
        Assert.AreEqual(expectedRoot, actualRoot);
    }

    private ModesColumnVariant Modes(int defaultIndex, params ModeColumnVariant[] modes)
    {
        return new ModesColumnVariant(modes, defaultIndex);
    }

    private ModeColumnVariant Mode(ColumnVariantIdentifier identifier, IColumnVariant subVariant = null)
    {
        return new ModeColumnVariant(identifier, null, subVariant ?? NullColumnVariant.Instance);
    }

    private ModesToggleColumnVariant ModesToggle(string toggleText, ModesColumnVariant modes)
    {
        return new ModesToggleColumnVariant(toggleText, modes);
    }

    private ToggleableColumnVariant Toggle(ColumnVariantIdentifier identifier, IColumnVariant subVariant = null)
    {
        return new ToggleableColumnVariant(identifier, null, subVariant ?? NullColumnVariant.Instance);
    }
}