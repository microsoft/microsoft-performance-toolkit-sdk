// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests
{
    public class FakeTableProvider
        : ITableProvider
    {
        public FakeTableProvider()
        {
            this.DiscoverCalls = new List<ISerializer>();
            this.DiscoverReturnValue = Array.Empty<DiscoveredTable>();
        }

        public List<ISerializer> DiscoverCalls { get; }

        public IEnumerable<DiscoveredTable> DiscoverReturnValue { get; set; }

        public IEnumerable<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
        {
            this.DiscoverCalls.Add(tableConfigSerializer);
            return this.DiscoverReturnValue;
        }
    }
}
