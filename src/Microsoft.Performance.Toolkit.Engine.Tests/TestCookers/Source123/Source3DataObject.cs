// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    public struct Source3DataObject
        : IKeyedDataObject<int>
    {
        public int Key => this.Id;

        public int Id { get; set; }

        public string Data { get; set; }

        public int GetKey()
        {
            return this.Key;
        }
    }
}
