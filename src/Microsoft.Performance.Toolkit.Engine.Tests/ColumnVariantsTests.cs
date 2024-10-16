// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestTables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests;

[TestClass]
public class ColumnVariantsTests
    : EngineFixture
{
    private DataSourceSet DefaultSet { get; set; }

    public override void OnInitialize()
    {
        this.DefaultSet = DataSourceSet.Create();
        base.OnInitialize();
    }

    public override void OnCleanup()
    {
        this.DefaultSet.SafeDispose();
    }

    [TestMethod]
    [IntegrationTest]
    public void SpecifiedVariantIsFound()
    {
        using var sut = Engine.Create(new EngineCreateInfo(this.DefaultSet.AsReadOnly()));

        sut.EnableTable(TableWithColumnVariants.TableDescriptor);

        var result = sut.Process();

        var builtTable = result.BuildTable(TableWithColumnVariants.TableDescriptor);

        Assert.AreEqual(2, builtTable.Columns.Count);
        Assert.AreEqual(1, builtTable.ColumnVariants.Keys.Count());

        var success = builtTable.TryGetColumnVariant(
            TableWithColumnVariants.ColumnOne.Metadata.Guid,
            TableWithColumnVariants.VariantIdentifier.Guid,
            out var variant);

        Assert.IsTrue(success, $"Failed to get variant {TableWithColumnVariants.VariantIdentifier.Guid} for column {TableWithColumnVariants.ColumnOne.Metadata.Guid}");
        Assert.IsTrue(((IDataColumn<bool>)variant).ProjectTyped(0));
    }
}