// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.Tests.Fixtures;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ColumnConfiguration = Microsoft.Performance.SDK.Processing.ColumnConfiguration;
using ColumnMetadata = Microsoft.Performance.SDK.Processing.ColumnMetadata;
using Projection = Microsoft.Performance.SDK.Processing.Projection;

namespace Microsoft.Performance.SDK.Runtime.Tests.ColumnBuilding;

[TestClass]
[UnitTest]
public class ToggledColumnWithToggledModesBuilderTests
{
    [TestMethod]
    public void WithToggle_NullIdentifierThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithToggle(null, Projection.Constant(1f)).Build();
        });
    }

    [TestMethod]
    public void WithToggle_NullProjectionThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            builder.WithToggle<int>(new ColumnVariantIdentifier(Guid.NewGuid(), "Foo"), null).Build();
        });
    }

    [TestMethod]
    public void WithToggledModes_NullTextThrows()
    {
        var builder = CreateSut();

        Assert.ThrowsException<ArgumentNullException>(() => { builder.WithToggledModes(null, _ => { }).Build(); });
    }

    [TestMethod]
    public void WithToggledModes_NullBuilderDoesNotThrow()
    {
        var builder = CreateSut();
        builder.WithToggledModes("foo", null).Build();
        Assert.IsTrue(true);
    }

    private ToggledColumnBuilder CreateSut()
    {
        var baseColumn = new DataColumn<float>(
            new ColumnConfiguration(
                new ColumnMetadata(Guid.NewGuid(), "base")), Projection.Constant<int, float>(0f));

        var initialToggle = new DataColumn<int>(
            new ColumnConfiguration(
                new ColumnMetadata(Guid.NewGuid(), "toggle")), Projection.Constant<int, int>(1));

        return new ToggledColumnWithToggledModesBuilder(
            new []{ new ToggledColumnBuilder.AddedToggle(new ColumnVariantIdentifier(Guid.NewGuid(), "Foo"), initialToggle) },
            baseColumn,
            new TestColumnVariantsProcessor(),
            new ModesBuilderCallbackInvoker((_) => { }, baseColumn),
            "Modes");
    }
}