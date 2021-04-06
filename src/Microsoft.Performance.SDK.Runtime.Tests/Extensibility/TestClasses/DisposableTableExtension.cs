// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class DisposableTableExtension
        : ITableExtension,
          IDisposable
    {
        public int DisposeCalls { get; set; }

        public TableDescriptor TableDescriptor { get; set; }

        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction { get; set; }

        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc { get; set; }

        public void Dispose()
        {
            ++this.DisposeCalls;
        }
    }
}
