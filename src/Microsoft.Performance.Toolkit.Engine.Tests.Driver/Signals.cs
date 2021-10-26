// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    internal static class Signals
    {
        internal static CancellationToken CancellationToken
        {
            get
            {
                Debug.Assert(Cts != null);
                return Cts.Token;
            }
        }

        private static CancellationTokenSource Cts { get; set; }

        internal static void Initialize()
        {
            Debug.Assert(Cts is null);
            Cts = new CancellationTokenSource();
        }

        internal static void Cleanup()
        {
            Cts?.Dispose();
            Cts = null;
        }

        internal static void Cancel()
        {
            Debug.Assert(Cts != null);
            Cts.Cancel();
        }
    }
}
