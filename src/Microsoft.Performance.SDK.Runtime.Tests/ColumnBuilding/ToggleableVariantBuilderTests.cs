// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.ColumnCommands;
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
public class ToggleableVariantBuilderTests
{
    private static readonly ColumnVariantDescriptor toggleDescriptor = new(Guid.NewGuid(), new ColumnVariantProperties { Label = "Toggle" });
    private static readonly IProjection<int, int> toggleProjection = Projection.Constant<int, int>(1);

    [TestMethod]
    public void WithCommands_ReturnsSameBuilderInstance()
    {
        var builder = new ToggledVariantBuilder<int>(toggleDescriptor, toggleProjection);
        var result = builder.WithCommands(new DataColumnCommands(new StubDownloadCommand()));
        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void WithCommands_CommandsPreservedInVariant()
    {
        var commands = new DataColumnCommands(new StubDownloadCommand());

        var variant = new ToggledVariantBuilder<int>(toggleDescriptor, toggleProjection)
            .WithCommands(commands)
            .CreateVariant(CreateBaseColumn());

        var columnWithCommands = variant.Column as IDataColumnWithCommands;
        Assert.IsNotNull(columnWithCommands);
        Assert.AreSame(commands, columnWithCommands.Commands);
    }

    [TestMethod]
    public void NoCommands_ExposesEmptyCommands()
    {
        var variant = new ToggledVariantBuilder<int>(toggleDescriptor, toggleProjection)
            .CreateVariant(CreateBaseColumn());

        var columnWithCommands = variant.Column as IDataColumnWithCommands;
        Assert.IsNotNull(columnWithCommands);
        Assert.AreSame(DataColumnCommands.Empty, columnWithCommands.Commands);
    }

    [TestMethod]
    public void Hierarchical_WithCommands_CommandsPreservedInVariant()
    {
        var commands = new DataColumnCommands(new StubDownloadCommand());

        var variant = new ToggledVariantBuilder<int>(toggleDescriptor, toggleProjection, new StubCollectionAccessProvider<int>())
            .WithCommands(commands)
            .CreateVariant(CreateBaseColumn());

        Assert.IsInstanceOfType(variant.Column, typeof(HierarchicalDataColumn<int>));

        var columnWithCommands = variant.Column as IDataColumnWithCommands;
        Assert.IsNotNull(columnWithCommands);
        Assert.AreSame(commands, columnWithCommands.Commands);
    }

    private static IDataColumn CreateBaseColumn()
    {
        return new DataColumn<int>(
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "base")),
            Projection.Constant<int, int>(0));
    }

    private sealed class StubDownloadCommand
        : DownloadSourceCodeCommand
    {
        public StubDownloadCommand()
            : base("Stub")
        {
        }

        public override bool CanExecute(object value, string downloadPath)
        {
            return false;
        }

        public override Task<DownloadSourceCodeResult[]> ExecuteAsync(
            object value,
            string downloadPath,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Array.Empty<DownloadSourceCodeResult>());
        }
    }
}
