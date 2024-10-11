// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Processing.DataColumns;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;


[TestClass]
[UnitTest]
public class RootColumnBuilderTests
{
    private class ExpectedColumnVariant
    {
        public ColumnVariantIdentifier Identifier { get; init; }
        public ExpectedColumnVariants SubVariants { get; init; }
    }
    private class ExpectedColumnVariants
    {
        public ColumnVariantsType Type { get; init; }
        public ExpectedColumnVariant[] PossibleVariants { get; init; }
    }

    static ColumnConfiguration baseConfig = new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "BaseColumn"));
    static IProjection<int, Timestamp> baseProj = Projection.Constant(Timestamp.Zero);

    static ColumnVariantIdentifier projectAsDateTime = new ColumnVariantIdentifier(Guid.NewGuid(), "Project as DateTime");

    static ColumnVariantIdentifier utc = new ColumnVariantIdentifier(Guid.NewGuid(), "UTC");
    static IProjection<int, DateTime> utcProj = Projection.Constant(DateTime.UtcNow);

    static ColumnVariantIdentifier showFloat = new ColumnVariantIdentifier(Guid.NewGuid(), "Local");
    static IProjection<int, float> floatProj = Projection.Constant(1f);

    [TestMethod]
    public void SingleToggle_UntoggledColumnHasBaseProjection()
    {
        TableBuilder builder = new TableBuilder();
        builder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .Build();
            });

        var expected = new ExpectedColumnVariants()
        {
            Type = ColumnVariantsType.Toggles,
            PossibleVariants = new[] { new ExpectedColumnVariant() { Identifier = projectAsDateTime } },
        };

        AssertCorrectColumnVariants(expected, builder.ColumnVariants[baseConfig]);
    }

    [TestMethod]
    public void Modes()
    {
        TableBuilder builder = new TableBuilder();
        builder
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

        var expected = new ExpectedColumnVariants()
        {
            Type = ColumnVariantsType.Modes,
            PossibleVariants = new[] { new ExpectedColumnVariant() { Identifier = projectAsDateTime }, new ExpectedColumnVariant() { Identifier = utc } },
        };

        AssertCorrectColumnVariants(expected, builder.ColumnVariants[baseConfig]);
    }

    [TestMethod]
    public void NestedToggle()
    {
        TableBuilder builder = new TableBuilder();
        builder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(utc, utcProj)
                    .Build();
            });

        var expected = new ExpectedColumnVariants()
        {
            Type = ColumnVariantsType.Toggles,
            PossibleVariants = new[]
            {
                new ExpectedColumnVariant()
                {
                    Identifier = projectAsDateTime,
                    SubVariants = new ExpectedColumnVariants()
                    {
                        Type = ColumnVariantsType.Toggles,
                        PossibleVariants = new[]
                        {
                            new ExpectedColumnVariant()
                            {
                                Identifier = utc,
                            }
                        },
                    },
                }
            },
        };

        AssertCorrectColumnVariants(expected, builder.ColumnVariants[baseConfig]);
    }

    private void AssertCorrectColumnVariants(ExpectedColumnVariants expected, IDataColumnVariants builderColumnVariant)
    {
        Assert.AreEqual(expected.Type, builderColumnVariant.Type);
        Assert.AreEqual(expected.PossibleVariants.Length, builderColumnVariant.PossibleVariants.Length);
        Assert.IsTrue(expected.PossibleVariants.SequenceEqual(builderColumnVariant.PossibleVariants));

        if (expected.SubVariants == null || builderColumnVariant.SubVariants == null)
        {
            Assert.IsNull(expected.SubVariants);
            Assert.IsNull(builderColumnVariant.SubVariants);
        }
        else
        {
            AssertCorrectColumnVariants(expected.SubVariants, builderColumnVariant.SubVariants);
        }
    }

    /*[TestMethod]
    public void SingleToggle_ApplyChangesToToggledProjection()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        dynamicDataColumn.Variants.PossibleVariants.First().TryApply();

        Assert.AreEqual(utcProj.ResultType, dynamicDataColumn.CurrentColumn.DataType);
    }

    [TestMethod]
    public void SingleToggle_UnapplyingChangesBackToBaseProjection()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        dynamicDataColumn.Variants.PossibleVariants.First().TryApply();
        dynamicDataColumn.Variants.PossibleVariants.First().TryUnapply();

        Assert.AreEqual(baseProj.ResultType, dynamicDataColumn.CurrentColumn.DataType);
    }

    [TestMethod]
    public void SingleToggle_InitialCannotUnapply()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        Assert.IsFalse(dynamicDataColumn.Variants.PossibleVariants.First().CanUnapply);
    }

    [TestMethod]
    public void SingleToggle_InitialIsUnapplied()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        Assert.IsFalse(dynamicDataColumn.Variants.PossibleVariants.First().IsApplied);
    }

    [TestMethod]
    public void SingleToggle_AppliedCanUnapply()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        dynamicDataColumn.Variants.PossibleVariants.First().TryApply();

        Assert.IsTrue(dynamicDataColumn.Variants.PossibleVariants.First().CanUnapply);
    }

    [TestMethod]
    public void SingleToggle_AppliedIsApplied()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .Run();

        dynamicDataColumn.Variants.PossibleVariants.First().TryApply();

        Assert.IsTrue(dynamicDataColumn.Variants.PossibleVariants.First().IsApplied);
    }

    [TestMethod]
    public void NestedToggle_UntoggledColumnHasBaseProjection()
    {
        var dynamicDataColumn = new TestBuilder()
            .AddToggleLayer(projectAsDateTime, utcProj)
            .AddToggleLayer(showFloat, floatProj)
            .Run();

        dynamicDataColumn.Variants.PossibleVariants.First().TryApply();

        Assert.IsTrue(dynamicDataColumn.Variants.PossibleVariants.First().IsApplied);

        new TableBuilder()
            .SetRowCount(1)
            .AddColumn(baseConfig, utcProj, options =>
            {
                options.SetToggle(projectAsDateTime, utcProj, innerOptions =>
                {
                    innerOptions.SetToggle(showFloat, floatProj);
                });
            });
    }*/
}