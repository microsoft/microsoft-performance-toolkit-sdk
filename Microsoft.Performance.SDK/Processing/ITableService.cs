// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Presents a way for a caller to be informed that a
    ///     table's data has changed, and give the caller a opportunity to
    ///     rebuild the table.
    /// </summary>
    public interface ITableService 
        : IDisposable
    {
        /// <summary>
        ///     Event that is raised when the current data in the table
        ///     is no longer valid.
        /// </summary>
        event EventHandler Invalidated;

        /// <summary>
        ///     This method is used by the caller when the processor has come
        ///     back into focus. This occurs when the user changes sessions,
        ///     and thus this method is called for every processor in the
        ///     session to which the user has selected. Here you would do
        ///     anything that might be required to determine if your data is
        ///     invalid, raise <see cref="Invalidated"/>, etc.
        /// </summary>
        void OnProcessorConnected();
    }
}
