// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestProcessingSourceCatalog
        : IProcessingSourceCatalog
    {
        public TestProcessingSourceCatalog()
        {
            this.ProcessingSources = Enumerable.Empty<ProcessingSourceReference>();
        }

        public bool IsLoaded { get; set; }

        public IEnumerable<ProcessingSourceReference> ProcessingSources { get; set; }

        public void Dispose()
        {
            this.ProcessingSources?.ForEach(x => x.SafeDispose());
        }
    }
}
