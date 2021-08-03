// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class DisposableCompositeDataCooker
        : TestCompositeDataCooker,
          IDisposable
    {
        public DisposableCompositeDataCooker()
        {
            this.Path = DataCookerPath.ForComposite(nameof(DisposableCompositeDataCooker));
            this.RequiredDataCookers = new List<DataCookerPath>();
        }

        public int DisposeCalls { get; set; }

        public void Dispose()
        {
            ++this.DisposeCalls;
        }
    }
}
