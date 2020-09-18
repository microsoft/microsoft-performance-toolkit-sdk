// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    internal class TestTableExtensionReference
        : TestDataExtensionReference,
          ITableExtensionReference
    {
        public TestTableExtensionReference()
            :base()
        {
        }

        public TestTableExtensionReference(bool useDataExtensionDependencyState)
            :base(useDataExtensionDependencyState)
        {
        }

        public TableDescriptor TableDescriptor { get; set; }

        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction { get; set; }

        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc { get; set; }

        public override IReadOnlyCollection<DataCookerPath> RequiredDataCookers => this.TableDescriptor.RequiredDataCookers;

        public override IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => this.TableDescriptor.RequiredDataProcessors;
    }
}
