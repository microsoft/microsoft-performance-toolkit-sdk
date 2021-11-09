// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public sealed class TestProcessorEnvironment
        : IProcessorEnvironment
    {
        public void AddNewTable(IDynamicTableBuilder dynamicTableBuilder)
        {
        }

        public Dictionary<Type, TestLogger> TestLoggers = new Dictionary<Type, TestLogger>();
        public ILogger CreateLogger(Type processorType)
        {
            if (TestLoggers.TryGetValue(processorType, out var testLogger))
            {
                return testLogger;
            }

            return new NullLogger();
        }

        public IDynamicTableBuilder RequestDynamicTableBuilder(TableDescriptor descriptor)
        {
            return new DynamicTableBuilder(AddNewTable, descriptor, new DataSourceInfo(0, 0, DateTime.UnixEpoch));
        }
    }
}
