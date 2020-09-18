// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing.SDK;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public sealed class TestProcessorEnvironment
        : IProcessorEnvironment
    {
        public void AddNewTable(IDynamicTableBuilder dynamicTableBuilder)
        {
        }

        public Func<ICustomDataProcessorWithSourceParser, IDataProcessorExtensibilitySupport> CreateDataProcessorExtensibilitySupportFunc
            { get; set; }

        public IDataProcessorExtensibilitySupport CreateDataProcessorExtensibilitySupport(ICustomDataProcessorWithSourceParser processor)
        {
            return CreateDataProcessorExtensibilitySupportFunc?.Invoke(processor);
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
