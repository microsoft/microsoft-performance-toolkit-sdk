// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestPluginCatalog
        : IPlugInCatalog
    {
        public TestPluginCatalog()
        {
            this.PlugIns = Enumerable.Empty<CustomDataSourceReference>();
        }

        public bool IsLoaded { get; set; }

        public IEnumerable<CustomDataSourceReference> PlugIns { get; set; }

        public void Dispose()
        {
            this.PlugIns?.ForEach(x => x.SafeDispose());
        }

        public IEnumerator<CustomDataSourceReference> GetEnumerator()
        {
            return this.PlugIns?.GetEnumerator() ?? 
                Enumerable.Empty<CustomDataSourceReference>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
