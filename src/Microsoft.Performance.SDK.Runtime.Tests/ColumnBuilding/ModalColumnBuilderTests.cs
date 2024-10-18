// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
public class ModalColumnBuilderTests
{
    private static ColumnVariantDescriptor modeDescriptor = new(Guid.NewGuid(), "Foo");
    private static IProjection<int, int> modeProjection = Projection.Constant<int, int>(1);

    [TestMethod]
    public void WithMode_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithMode(null, Projection.Constant(1f));
        });
    }

    [TestMethod]
    public void WithMode_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentNullException>(() =>
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

        Assert.ThrowsException<ArgumentException>(() =>
        {
            builder.WithDefaultMode(Guid.NewGuid());
        });
    }

    private ModalColumnBuilder CreateSut()
    {
        return new ModalColumnWithModesBuilder(
            new TestColumnVariantsProcessor(),
            new List<ModalColumnWithModesBuilder.AddedMode>(),
            new DataColumn<int>(
                new ColumnConfiguration(
                    new ColumnMetadata(Guid.NewGuid(), "foo")), Projection.Constant<int, int>(1)),
            null);
    }
}