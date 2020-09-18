// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    public class TestProgress
        : IProgress<int>
    {
        public List<int> ReportedValues { get; set; } = new List<int>(100);

        public void Report(int value)
        {
            this.ReportedValues.Add(value);
        }
    }
}
