// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    /// <summary>
    ///     Contains methods for calling into Native Code.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///     Determines whether the current process is being run under a debugger.
        /// </summary>
        /// <remarks>
        ///     Debugger.IsAttached only works for managed debuggers.
        ///     This method is used to detect if *ANY* debugger is attached (i.e. WinDbg.)
        /// </remarks>
        /// <returns>
        ///     <c>true</c> if the current process is running under a debugger; <c>false</c>
        ///     otherwise.
        /// </returns>
        [DllImport("Kernel32.dll", ExactSpelling = true)]
        internal static extern bool IsDebuggerPresent();

        /// <summary>
        ///     Triggers a breakpoint exception in the current process.
        /// </summary>
        [DllImport("Kernel32.dll", ExactSpelling = true)]
        internal static extern void DebugBreak();
    }
}
