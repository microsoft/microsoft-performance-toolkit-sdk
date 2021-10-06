// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    internal sealed class ThunkedTableProvider
        : ITableProvider
    {
        private readonly Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> getTables;

        internal ThunkedTableProvider(
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> getTables)
        {
            this.getTables = getTables;
        }

        public ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
        {
            var dt = new HashSet<DiscoveredTable>();
            foreach (var kvp in this.getTables())
            {
                dt.Add(new DiscoveredTable(kvp.Key, kvp.Value));
            }

            return dt;
        }
    }
}
