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
    private static ColumnVariantIdentifier modeIdentifier = new(Guid.NewGuid(), "Foo");
    private static IProjection<int, int> modeProjection = Projection.Constant<int, int>(1);

    [TestMethod]
    public void WithMode_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithMode(null, Projection.Constant(1f)).Build();
        });
    }

    [TestMethod]
    public void WithMode_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithMode<int>(modeIdentifier, null).Build();
        });
    }

    [TestMethod]
    public void WithMode_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithMode(modeIdentifier, modeProjection, null).Build();
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
                new StubCollectionAccessProvider<float>())
                .Build();
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalMode<float>(
                modeIdentifier,
                null,
                new StubCollectionAccessProvider<float>())
                .Build();
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullCollectionInfoThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithHierarchicalMode<float>(
                modeIdentifier,
                Projection.Constant(1f),
                null)
                .Build();
        });
    }

    [TestMethod]
    public void WithHierarchicalMode_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithHierarchicalMode(
            modeIdentifier,
            modeProjection,
            new StubCollectionAccessProvider<int>(),
            null)
            .Build();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void WithDefaultMode_UnregisteredGuidThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentException>(() =>
        {
            builder.WithDefaultMode(Guid.NewGuid()).Build();
        });
    }

    private ModalColumnBuilder CreateSut()
    {
        return new ModalColumnBuilder(
            new TestColumnVariantsProcessor(),
            new List<ModalColumnBuilder.AddedMode>(),
            new DataColumn<int>(
                new ColumnConfiguration(
                    new ColumnMetadata(Guid.NewGuid(), "foo")), Projection.Constant<int, int>(1)),
            null);
    }
}