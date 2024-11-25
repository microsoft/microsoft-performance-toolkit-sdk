// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.Tests.Fixtures;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ColumnConfiguration = Microsoft.Performance.SDK.Processing.ColumnConfiguration;
using ColumnMetadata = Microsoft.Performance.SDK.Processing.ColumnMetadata;
using Projection = Microsoft.Performance.SDK.Processing.Projection;

namespace Microsoft.Performance.SDK.Runtime.Tests.ColumnBuilding;

[TestClass]
[UnitTest]
public class ToggledColumnBuilderTests
{
    [TestMethod]
    public void WithToggle_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithToggle(null, Projection.Constant(1f));
        });
    }

    [TestMethod]
    public void WithToggle_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithToggle<int>(new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }), null);
        });
    }

    [TestMethod]
    public void WithHierarchicalToggle_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() => { builder.WithToggledModes(null, modesBuilder => modesBuilder); });
    }

    [TestMethod]
    public void WithToggledModes_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithToggledModes("foo", null);
        Assert.IsTrue(true);
    }

    protected virtual ToggleableColumnBuilder CreateSut()
    {
        var baseColumn = new DataColumn<float>(
            new ColumnConfiguration(
                new ColumnMetadata(Guid.NewGuid(), "base")), Projection.Constant<int, float>(0f));

        var initialToggle = new DataColumn<int>(
            new ColumnConfiguration(
                new ColumnMetadata(Guid.NewGuid(), "toggle")), Projection.Constant<int, int>(1));

        return new ToggledColumnBuilder(
            new []{ new ToggledColumnBuilder.AddedToggle(new ColumnVariantDescriptor(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" }), initialToggle) },
            baseColumn,
            new TestColumnVariantsProcessor());
    }
}