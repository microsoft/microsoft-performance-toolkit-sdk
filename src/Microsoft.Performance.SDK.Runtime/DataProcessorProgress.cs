// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Handles progress callbacks while processing the data sources.
    /// </summary>
    public sealed class DataProcessorProgress
        : IProgress<int>,
          IProgressUpdate
    {
        /// <summary>
        ///     Event to be notified when properties change on <see cref="DataProcessorProgress"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int currentProgress;
        
        public int CurrentProgress
        {
            get
            {
                return this.currentProgress;
            }
            set
            {
                if (value != this.currentProgress)
                {
                    this.currentProgress = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(CurrentProgressString));
                }
            }
        }

        private ProcessorStatus currentStatus;

        public ProcessorStatus CurrentStatus
        {
            get
            {
                return this.currentStatus;
            }
            set
            {
                if (value != this.currentStatus)
                {
                    this.currentStatus = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string CurrentProgressString => this.CurrentProgress.ToString() + "%";

        public void Report(int progress)
        {
            this.CurrentProgress = progress;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
