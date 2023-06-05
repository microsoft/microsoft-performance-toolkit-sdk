// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;

namespace Microsoft.Performance.SDK.Runtime.Tests.Fixtures;

/// <summary>
///     A <see cref="SynchronizationContext"/> that runs everything immediately. Useful for testing methods involving
///     async operations such as async event handler callbacks.
/// </summary>
public sealed class TestSynchronizationContext
    : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object state)
    {
        d(state);
    }
    public override void Send(SendOrPostCallback d, object state)
    {
        d(state);
    }
}
