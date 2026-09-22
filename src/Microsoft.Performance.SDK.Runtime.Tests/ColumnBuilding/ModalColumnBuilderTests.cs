// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.Tests.Fixtures;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ColumnConfiguration = Microsoft.Performance.SDK.Processing.ColumnConfiguration;
using ColumnMetadata = Microsoft.Performance.SDK.Processing.ColumnMetadata;
using Projection = Microsoft.Performance.SDK.Processing.Projection;

namespace Microsoft.Performance.SDK.Runtime.Tests.ColumnBuilding;

[TestClass]
[UnitTest]
public class ModalColumnBuilderTests
{
    private static ColumnVariantDescriptor modeDescriptor = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Foo" });
    private static IProjection<int, int> modeProjection = Projection.Constant<int, int>(1);

    [TestMethod]
    public void WithMode_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithMode(null, Projection.Constant(1f));
        });
    }

    [TestMethod]
    public void WithMode_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithMode<int>(modeDescriptor, null);
        });
    }

    [TestMethod]
    public void WithMode_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithMode(modeDescriptor, modeProjection, null);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void WithHierarchicalMode_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalMode(
                null,
                Projection.Constant(1f),
                new StubCollectionAccessProvider<float>());
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalMode<float>(
                modeDescriptor,
                null,
                new StubCollectionAccessProvider<float>());
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullCollectionInfoThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalMode<float>(
                modeDescriptor,
                Projection.Constant(1f),
                null);
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithHierarchicalMode(
            modeDescriptor,
            modeProjection,
            new StubCollectionAccessProvider<int>(),
            null);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void WithDefaultMode_UnregisteredGuidThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            builder.WithDefaultMode(Guid.NewGuid());
        });
    }

    [TestMethod]
    public void WithModalBuilder_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithModalBuilder(null, modeProjection, variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithModalBuilder_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithModalBuilder<int>(modeDescriptor, null, variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithModalBuilder_NullBuildVariantThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithModalBuilder(modeDescriptor, modeProjection, null);
        });
    }

    [TestMethod]
    public void WithHierarchicalModalBuilder_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalModalBuilder(
                null,
                modeProjection,
                new StubCollectionAccessProvider<int>(),
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalModalBuilder_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalModalBuilder<int>(
                modeDescriptor,
                null,
                new StubCollectionAccessProvider<int>(),
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalModalBuilder_NullCollectionInfoThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalModalBuilder<int>(
                modeDescriptor,
                modeProjection,
                null,
                variantBuilder => variantBuilder);
        });
    }

    [TestMethod]
    public void WithHierarchicalModalBuilder_NullBuildVariantThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalModalBuilder<int>(
                modeDescriptor,
                modeProjection,
                new StubCollectionAccessProvider<int>(),
                null);
        });
    }

    private ModalColumnBuilder CreateSut()
    {
        return new ModalColumnWithModesBuilder(
            new TestColumnVariantsProcessor(),
            new List<ModalVariant>(),
            new DataColumn<int>(
                new ColumnConfiguration(
                    new ColumnMetadata(Guid.NewGuid(), "foo")), Projection.Constant<int, int>(1)),
            null);
    }
}