// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
            this.DiscoverReturnValue = new HashSet<DiscoveredTable>();
        }

        public List<ISerializer> DiscoverCalls { get; }

        public ISet<DiscoveredTable> DiscoverReturnValue { get; set; }

        public ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
        {
            this.DiscoverCalls.Add(tableConfigSerializer);
            return this.DiscoverReturnValue;
        }
    }
}
