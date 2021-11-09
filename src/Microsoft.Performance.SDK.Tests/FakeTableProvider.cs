// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests
{
    public class FakeTableProvider
        : IProcessingSourceTableProvider
    {
        public FakeTableProvider()
        {
            this.DiscoverCalls = new List<ITableConfigurationsSerializer>();
            this.DiscoverReturnValue = Array.Empty<TableDescriptor>();
        }

        public List<ITableConfigurationsSerializer> DiscoverCalls { get; }

        public IEnumerable<TableDescriptor> DiscoverReturnValue { get; set; }

        public IEnumerable<TableDescriptor> Discover(ITableConfigurationsSerializer tableConfigSerializer)
        {
            this.DiscoverCalls.Add(tableConfigSerializer);
            return this.DiscoverReturnValue;
        }
    }
}
