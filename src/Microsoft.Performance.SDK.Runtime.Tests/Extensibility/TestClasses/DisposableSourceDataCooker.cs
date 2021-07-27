// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class DisposableSourceDataCooker
        : TestSourceDataCooker,
          IDisposable
    {
        public DisposableSourceDataCooker()
        {
            this.Path = DataCookerPath.ForComposite(nameof(DisposableSourceDataCooker));
            this.RequiredDataCookers = new List<DataCookerPath>();
        }

        public int DisposeCalls { get; set; }

        public void Dispose()
        {
            ++this.DisposeCalls;
        }
    }
}
