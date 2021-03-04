// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakePreloadValidator
        : IPreloadValidator
    {
        public bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error)
        {
            error = ErrorInfo.None;
            return true;
        }

        public void Dispose()
        {
        }
    }
}
