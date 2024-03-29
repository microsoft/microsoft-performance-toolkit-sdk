// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestCookedDataRetrieval
        : ICookedDataRetrieval
    {
        public T QueryOutput<T>(DataOutputPath identifier)
        {
            return (T)QueryOutput(identifier);
        }

        public Func<DataOutputPath, object> queryOutputFunc;
        public object QueryOutput(DataOutputPath identifier)
        {
            return this.queryOutputFunc?.Invoke(identifier);
        }

        public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        {
            result = default;
            return false;
        }

        public bool TryQueryOutput(DataOutputPath identifier, out object result)
        {
            result = default;
            return false;
        }
    }
}
