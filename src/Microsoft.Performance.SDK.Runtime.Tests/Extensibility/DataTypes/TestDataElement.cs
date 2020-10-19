// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes
{
    public class TestDataElement
        : IKeyedDataType<int>
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public byte[] Data { get; set; }

        public int CompareTo(int other)
        {
            return Id.CompareTo(other);
        }

        public int GetKey()
        {
            return Id;
        }
    }
}
