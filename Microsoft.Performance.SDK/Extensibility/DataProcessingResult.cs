// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     The result from processing data from a source.
    /// </summary>
    public enum DataProcessingResult
    {
        /// <summary>
        ///     The data was successfully processed.
        /// </summary>
        Processed,

        /// <summary>
        ///     The data was ignored by the data processor.
        /// </summary>
        Ignored,

        /// <summary>
        ///     The data was reported corrupt by the data processor.
        /// </summary>
        CorruptData,

        /// <summary>
        ///     The version of the event is not supported.
        /// </summary>
        InvalidVersion,
    }
}
