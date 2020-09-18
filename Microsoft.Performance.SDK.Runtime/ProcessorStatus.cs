// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents the runtime status of an instance
    ///     of a custom data processor.
    /// </summary>
    public enum ProcessorStatus
    {
        /// <summary>
        ///     The processor is not running, and has not
        ///     yet been run.
        /// </summary>
        Idle,

        /// <summary>
        ///     The processor is currently processing data.
        /// </summary>
        Running,

        /// <summary>
        ///     The processor has run, terminated, and was
        ///     successful.
        /// </summary>
        Succeeded,

        /// <summary>
        ///     The processor has run, terminated, and terminated
        ///     with a failure condition.
        /// </summary>
        Failed,

        /// <summary>
        ///     The processor was running but was terminated due to
        ///     a cancellation request.
        /// </summary>
        Cancelled,
    }
}
