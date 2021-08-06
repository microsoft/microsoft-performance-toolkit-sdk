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
            this.PlugIns = Enumerable.Empty<ProcessingSourceReference>();
        }

        public bool IsLoaded { get; set; }

        public IEnumerable<ProcessingSourceReference> PlugIns { get; set; }

        public void Dispose()
        {
            this.PlugIns?.ForEach(x => x.SafeDispose());
        }
    }
}
