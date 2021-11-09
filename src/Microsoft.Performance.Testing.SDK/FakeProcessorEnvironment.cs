// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeProcessorEnvironment
        : IProcessorEnvironment
    {
        public void AddNewTable(IDynamicTableBuilder dynamicTableBuilder)
        {
        }

        public ILogger CreateLogger(Type processorType)
        {
            return new NullLogger();
        }

        public IDynamicTableBuilder RequestDynamicTableBuilder(TableDescriptor descriptor)
        {
            return new DynamicTableBuilder(this.AddNewTable, descriptor, new DataSourceInfo(0, 0, DateTime.UnixEpoch));
        }
    }
}
