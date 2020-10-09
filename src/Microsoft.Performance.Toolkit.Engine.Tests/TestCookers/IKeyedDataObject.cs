// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers
{
    public interface IKeyedDataObject<TKey>
        : IKeyedDataType<TKey>
    {
        TKey Key { get; }
    }
}
