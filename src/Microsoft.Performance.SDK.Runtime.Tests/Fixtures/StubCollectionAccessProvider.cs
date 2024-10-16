// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Tests.Fixtures;

internal class StubCollectionAccessProvider<T>
    : ICollectionAccessProvider<T, T>
{
    public bool IsNull(T value)
    {
        throw new NotImplementedException();
    }

    public bool HasUniqueStart { get; }
    public int GetCount(T collection)
    {
        throw new NotImplementedException();
    }

    public bool Equals(T x, T y)
    {
        throw new NotImplementedException();
    }

    public int GetHashCode(T obj)
    {
        throw new NotImplementedException();
    }

    public T PastEndValue { get; }
    public T GetParent(T collection)
    {
        throw new NotImplementedException();
    }

    public T GetValue(T collection, int index)
    {
        throw new NotImplementedException();
    }
}