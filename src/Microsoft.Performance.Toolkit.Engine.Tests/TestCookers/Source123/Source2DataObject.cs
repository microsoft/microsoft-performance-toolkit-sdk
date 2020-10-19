// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    public struct Source2DataObject
        : IKeyedDataObject<int>
    {
        public int Key => this.Id;

        public int Id { get; set; }

        public string Data { get; set; }

        public int CompareTo(int other)
        {
            return this.Key.CompareTo(other);
        }

        public int GetKey()
        {
            return this.Key;
        }
    }
}
