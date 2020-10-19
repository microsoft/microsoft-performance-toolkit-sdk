// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes
{
    public class TestRecord
        : IKeyedDataType<int>
    {
        public int Key { get; set; }

        public string Value { get; set; }

        public int CompareTo(int otherKey)
        {
            return this.Key.CompareTo(otherKey);
        }

        public int GetKey()
        {
            return this.Key;
        }
    }
}
