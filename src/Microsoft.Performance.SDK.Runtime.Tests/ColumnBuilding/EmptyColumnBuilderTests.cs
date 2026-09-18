// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.Tests.Fixtures;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ColumnConfiguration = Microsoft.Performance.SDK.Processing.ColumnConfiguration;
using ColumnMetadata = Microsoft.Performance.SDK.Processing.ColumnMetadata;
using Projection = Microsoft.Performance.SDK.Processing.Projection;

namespace Microsoft.Performance.SDK.Runtime.Tests.ColumnBuilding;

[TestClass]
[UnitTest]
public class EmptyColumnBuilderTests
{
    [TestMethod]
    public void WithToggle_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithToggle(null, Projection.Constant(1f));
        });
    }

    [TestMethod]
    public void WithToggle_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithToggle<int>(new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }), null);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggle_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggle(
                null,
                Projection.Constant(1f),
                new StubCollectionAccessProvider<float>());
        });
    }

    [TestMethod]
    public void WithHierarchicalToggle_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggle(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                null,
                new StubCollectionAccessProvider<float>());
        });
    }

    [TestMethod]
    public void WithHierarchicalToggle_NullCollectionInfoThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggle(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                Projection.Constant(1f),
                null);
        });
    }

    [TestMethod]
    public void WithToggledModes_NullTextThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() => { builder.WithToggledModes(null, builder => builder); });
    }

    [TestMethod]
    public void WithToggledModes_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithToggledModes("Foo", null);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void WithModes_NullPropertiesThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() => { builder.WithModes(null, builder => builder); });
    }

    [TestMethod]
    public void WithModes_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithModes(new ColumnVariantProperties { Label = "Foo" });
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void WithToggleableBuilder_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithToggleableBuilder(null, Projection.Constant(1f), variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithToggleableBuilder_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithToggleableBuilder<int>(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                null,
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithToggleableBuilder_NullBuildVariantThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithToggleableBuilder(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                Projection.Constant(1f),
                null);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggleableBuilder_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggleableBuilder(
                null,
                Projection.Constant(1f),
                new StubCollectionAccessProvider<float>(),
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggleableBuilder_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggleableBuilder<float>(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                null,
                new StubCollectionAccessProvider<float>(),
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggleableBuilder_NullCollectionInfoThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggleableBuilder<float>(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                Projection.Constant(1f),
                null,
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggleableBuilder_NullBuildVariantThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalToggleableBuilder<float>(
                new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }),
                Projection.Constant(1f),
                new StubCollectionAccessProvider<float>(),
                null);
        });
    }

    private EmptyColumnBuilder CreateSut()
    {
        return new EmptyColumnBuilder(
            new TestColumnVariantsProcessor(),
            new DataColumn<int>(
                new ColumnConfiguration(
                    new ColumnMetadata(Guid.NewGuid(), "Foo")), Projection.Constant<int, int>(1)));
    }
}