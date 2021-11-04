using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Performance.SDK.Runtime
{
	/// <summary>
	///		This Progress reporter will update a listener everytime it is updated.
	///		It is provided the number of components it relies on.
	///		For example, if 
	/// </summary>
	public sealed class ProgressReporter
		: IProgress<int>,
		  IProgressUpdate

	{

		public event PropertyChangedEventHandler PropertyChanged;

		private readonly IProgress<int> listener;
		private readonly int numberOfParts;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="listener">Update this listener everytime this.currentProgress updates</param>
		/// <param name="numberOfParts">Number of times that this.currentProgress will reach 100 due to subcomp</param>
		public ProgressReporter(IProgress<int> listener, int numberOfParts = 1)
		{
			this.listener = listener;
			this.numberOfParts = numberOfParts;
			rawProgress = 0;
		}


		private int rawProgress;

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
					var prevProgres = this.currentProgress;
					this.currentProgress = value;

					// this makes an assumption that progress must always increase
					// progress only decreases once the next component beings
					if (this.currentProgress < prevProgres)
					{
						rawProgress += this.currentProgress;
					} else
					{
						rawProgress += this.currentProgress - prevProgres;
					}

					// update listener
					this.listener.Report(rawProgress / numberOfParts);
					
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
