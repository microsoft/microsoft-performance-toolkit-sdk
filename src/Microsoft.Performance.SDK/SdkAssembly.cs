// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     This class exists to provide a hook for getting the
    ///     SDK assembly.
    /// </summary>
    public static class SdkAssembly
    {
        /// <summary>
        ///     The Assembly containing this version of the SDK.
        /// </summary>
        public static readonly Assembly Assembly = typeof(SdkAssembly).Assembly;
    }
}
