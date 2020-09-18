// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Interface to notify of progress updates during data source processing.
    /// </summary>
    public interface IProgressUpdate
        : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets the current progress of processing.
        ///     <para />
        ///     Value is 0 - 100, where 100 represents complete.
        /// </summary>
        int CurrentProgress { get; }

        /// <summary>
        ///     Gets the <see cref="string"/> representation of the progress.
        ///     <para />
        ///     Format: <see cref="CurrentProgress"/> + %.
        /// </summary>
        string CurrentProgressString { get; }

        /// <summary>
        ///     Gets the current <see cref="ProcessorStatus"/> of the processing session.
        /// </summary>
        ProcessorStatus CurrentStatus { get; }
    }
}
