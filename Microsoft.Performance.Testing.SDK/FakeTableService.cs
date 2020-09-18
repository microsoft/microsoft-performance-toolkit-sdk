// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeTableService
        : ITableService
    {
#pragma warning disable 0067
        // this is a Fake, so we don't need to use this.
        public event EventHandler Invalidated;
#pragma warning restore 0067

        public void Dispose()
        {
        }

        public void OnProcessorConnected()
        {
            throw new NotImplementedException();
        }
    }
}
