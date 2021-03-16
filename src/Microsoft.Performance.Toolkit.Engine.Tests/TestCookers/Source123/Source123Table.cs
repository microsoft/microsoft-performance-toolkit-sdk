using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    public class Source123Table
    {
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{85CA4F92-4ECD-4EC4-B4B3-A98B373F29CA}"),
            "Processor Test Table",
            "Used by the Engine Tests to test a table via processor",
            "Engine");

        public static readonly ColumnConfiguration ColumnOne =
                new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "One"));

        public static readonly ColumnConfiguration ColumnTwo =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Two"));

        public static readonly ColumnConfiguration ColumnThree =
            new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Three"));

        public void Build(ITableBuilder tableBuilder)
        {
            tableBuilder.SetRowCount(1)
                .AddColumn(ColumnOne, Projection.Constant(1))
                .AddColumn(ColumnTwo, Projection.Constant(2))
                .AddColumn(ColumnThree, Projection.Constant(3));
        }
    }
}
