// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestTableExtensionReference
        : TestDataExtensionReference,
          ITableExtensionReference
    {
        public TestTableExtensionReference()
            : this(true)
        {
        }

        public TestTableExtensionReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState)
        {
            TableDescriptor = Any.TableDescriptor();
        }

        public TestTableExtensionReference(
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : this(true, createDependencyState)
        {
        }

        public TestTableExtensionReference(
            bool useDataExtensionDependencyState,
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : base(useDataExtensionDependencyState, createDependencyState)
        {
            TableDescriptor = Any.TableDescriptor();
        }

        public TableDescriptor TableDescriptor { get; set; }

        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction { get; set; }

        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc { get; set; }

        public override IReadOnlyCollection<DataCookerPath> RequiredDataCookers => TableDescriptor.RequiredDataCookers;

        // TODO: __SDK_DP__
        //public override IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => TableDescriptor.RequiredDataProcessors;

        public bool IsInternalTable { get; set; }
    }
}
