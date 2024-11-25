// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;

[TestClass]
[UnitTest]
public class ColumnVariantsTests
{
    private static readonly ColumnConfiguration baseConfig = new(new ColumnMetadata(Guid.NewGuid(), "BaseColumn"));
    private static readonly IProjection<int, Timestamp> baseProj = Projection.Constant(Timestamp.Zero);

    private static readonly ColumnVariantDescriptor projectAsDateTime = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Project as DateTime", Description = "Projects as datetime", ColumnName  = "Time (DateTime)"});

    private static readonly ColumnVariantDescriptor utc = new(baseConfig.Metadata.Guid, new ColumnVariantProperties { Label = "UTC", Description = "Projects to UTC", ColumnName = "Time (UTC)"});
    private static readonly IProjection<int, DateTime> utcProj = Projection.Constant(DateTime.UtcNow);

    private static readonly ColumnVariantDescriptor local = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Local", Description = "Projects to local time", ColumnName = "Time (Local)" });
    private static readonly IProjection<int, DateTime> localProj = Projection.Constant(DateTime.UtcNow.AddHours(2));

    private static readonly ColumnVariantDescriptor showFloat = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Float", Description = "Projects a float"  });
    private static readonly IProjection<int, float> floatProj = Projection.Constant(1f);

    private static readonly ColumnVariantDescriptor showBool = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Bool", Description = "Projects a bool"  });
    private static readonly IProjection<int, bool> boolProj = Projection.Constant(true);

    [TestMethod]
    public void BuildingTwiceOverridesFirstBuild()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder.SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                var builderWithOneToggle =
                    builder
                        .WithToggle(projectAsDateTime, localProj);

                builderWithOneToggle
                    .WithToggledModes("foo", modeBuilder =>
                    {
                        return modeBuilder
                            .WithMode(utc, utcProj)
                            .WithMode(showFloat, floatProj);
                    });

                // This should ignore everything above and only expose the single toggle
                return builderWithOneToggle;
            });

        var expected = Toggle(projectAsDateTime);
        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void SingleToggle()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, baseProj, builder =>
            {
                return builder
                    .WithToggle(projectAsDateTime, utcProj);
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
                return builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(utc, utcProj);
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
                return builder
                    .WithModes(utc.Properties)
                    .WithMode(local, localProj);
            });

        var expected = Modes(
            0,
            Mode(utc),
            Mode(local));

        AssertCorrectColumnVariants(expected, tableBuilder);
    }

    [TestMethod]
    public void BaseModeHasCorrectNameAndDescription()
    {
        ColumnVariantProperties baseModeProperties = new ColumnVariantProperties { Label = "Test Base Mode Name", Description = "Test Base Mode Name Description" };

        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                return builder
                    .WithModes(baseModeProperties)
                    .WithMode(local, localProj);
            });

        var actualRoot = GetTreeNode(tableBuilder);
        Assert.IsInstanceOfType(actualRoot, typeof(ModesColumnVariantsTreeNode));

        var baseMode = ((ModesColumnVariantsTreeNode)actualRoot).Modes.Cast<ModeColumnVariantsTreeNode>().Single(m => m.ModeDescriptor.Guid == baseConfig.Metadata.Guid);

        Assert.AreEqual(baseMode.ModeDescriptor.Properties.Label, baseModeProperties.Label);
        Assert.AreEqual(baseMode.ModeDescriptor.Properties.Description, baseModeProperties.Description);
    }

    [TestMethod]
    public void ModesWithDefaultIndex()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                return builder
                    .WithModes(utc.Properties)
                    .WithMode(local, localProj)
                    .WithDefaultMode(local.Guid);
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
                return builder
                    .WithModes(
                        utc.Properties,
                        modeBuilder =>
                        {
                            return modeBuilder
                                .WithToggle(showFloat, floatProj)
                                .WithToggle(showBool, boolProj);
                        })
                    .WithMode(local, localProj);
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
    public void ToggleWithNestedModes()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                return builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggledModes("withModes", modeBuilder =>
                    {
                        return modeBuilder
                            .WithMode(local, localProj);
                    });
            });

        var expected =
            Toggle(projectAsDateTime,
                ModesToggle("withModes",
                    Modes(
                        0,
                        Mode(local))));

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
                return builder
                    .WithModes(utc.Properties)
                    .WithMode(local, floatProj, modeBuilder =>
                    {
                        return modeBuilder.WithToggledModes(
                            "Foo",
                            nestedModes =>
                            {
                                return nestedModes
                                    .WithMode(showFloat, floatProj)
                                    .WithMode(showBool, boolProj);
                            });
                    });
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

    [TestMethod]
    public void TogglesWithSameGuid_ThrowsInvalidOperationException()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder.SetRowCount(1);

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            tableBuilder.AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                return builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(projectAsDateTime, localProj);
            });
        });
    }

    [TestMethod]
    public void AddColumnWithVariants_NullColumnThrows()
    {
        var tableBuilder = new TableBuilder().SetRowCount(1);

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            tableBuilder.AddColumnWithVariants(null, (builder) => builder);
        });
    }

    [TestMethod]
    public void DuplicateGuidsAcrossColumns_DoesNotThrow()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder.SetRowCount(1);

        tableBuilder.AddColumnWithVariants(baseConfig, utcProj, builder =>
        {
            return builder
                .WithToggle(projectAsDateTime, utcProj);
        });

        tableBuilder.AddColumnWithVariants(new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "other columns")), boolProj, builder =>
        {
            return builder
                .WithToggle(projectAsDateTime, utcProj);
        });

        Assert.IsTrue(true);
    }

    [TestMethod]
    public void ToggleAndModeWithSameGuid_ThrowsInvalidOperationException()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder.SetRowCount(1);

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            tableBuilder.AddColumnWithVariants(baseConfig, utcProj, builder =>
            {
                return builder
                    .WithToggle(projectAsDateTime, utcProj)
                    .WithToggle(showBool, boolProj)
                    .WithToggledModes("foo", modeBuilder =>
                    {
                        return modeBuilder
                            .WithMode(projectAsDateTime, localProj);
                    });
            });
        });
    }

    [TestMethod]
    public void AddColumnWithVariants_NullBuilderDoesNotThrow()
    {
        var tableBuilder = new TableBuilder().SetRowCount(1);
        tableBuilder.AddColumnWithVariants(baseConfig, utcProj, null);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void ToggleWithColumnName_HasCorrectColumnName()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(
            baseConfig,
            utcProj,
            builder => builder.WithToggle(projectAsDateTime, utcProj));

        var toggleNode = GetTreeNode(tableBuilder) as ToggleableColumnVariantsTreeNode;
        Assert.IsNotNull(toggleNode);
        Assert.AreEqual(projectAsDateTime.Properties.ColumnName, toggleNode.ToggledColumn.Configuration.Metadata.Name);
    }

    [TestMethod]
    public void ModeWithColumnName_HasCorrectColumnName()
    {
        var tableBuilder = new TableBuilder();
        tableBuilder
            .SetRowCount(1)
            .AddColumnWithVariants(
                baseConfig,
                utcProj,
                builder => builder
                    .WithToggledModes("As datetime",
                        modesBuilder => modesBuilder
                            .WithMode(projectAsDateTime, utcProj)
                            .WithMode(local, localProj)));

        var toggleNode = GetTreeNode(tableBuilder) as ModesToggleColumnVariantsTreeNode;
        Assert.IsNotNull(toggleNode);

        var utcMode = ((ModesColumnVariantsTreeNode)toggleNode.Modes).Modes.First() as ModeColumnVariantsTreeNode;
        Assert.AreEqual(projectAsDateTime.Properties.ColumnName, utcMode.ModeColumn.Configuration.Metadata.Name);

        var localMode = ((ModesColumnVariantsTreeNode)toggleNode.Modes).Modes.Last() as ModeColumnVariantsTreeNode;
        Assert.AreEqual(local.Properties.ColumnName, localMode.ModeColumn.Configuration.Metadata.Name);
    }

    private void AssertCorrectColumnVariants(
        IColumnVariantsTreeNode expectedRoot, TableBuilder builtTable)
    {
        var actualRoot = GetTreeNode(builtTable);
        Assert.IsTrue(expectedRoot.IsEquivalentTree(actualRoot));
    }

    private IColumnVariantsTreeNode GetTreeNode(TableBuilder builtTable)
    {
        var success = builtTable.ColumnVariantsRegistrar.TryGetVariantsTreeRoot(builtTable.Columns.First(), out var actualRoot);
        Assert.IsTrue(success, "No variants were registered for the column, but they were expected to be.");
        return actualRoot;
    }

    private ModesColumnVariantsTreeNode Modes(int defaultIndex, params ModeColumnVariantsTreeNode[] modes)
    {
        return new ModesColumnVariantsTreeNode(modes, defaultIndex);
    }

    private ModeColumnVariantsTreeNode Mode(ColumnVariantDescriptor descriptor, IColumnVariantsTreeNode subVariantsTreeNode = null)
    {
        return new ModeColumnVariantsTreeNode(descriptor, null, subVariantsTreeNode ?? NullColumnVariantsTreeNode.Instance);
    }

    private ModesToggleColumnVariantsTreeNode ModesToggle(string toggleText, ModesColumnVariantsTreeNode modes)
    {
        return new ModesToggleColumnVariantsTreeNode(toggleText, modes);
    }

    private ToggleableColumnVariantsTreeNode Toggle(ColumnVariantDescriptor descriptor, IColumnVariantsTreeNode subVariantsTreeNode = null)
    {
        return new ToggleableColumnVariantsTreeNode(descriptor, null, subVariantsTreeNode ?? NullColumnVariantsTreeNode.Instance);
    }
}