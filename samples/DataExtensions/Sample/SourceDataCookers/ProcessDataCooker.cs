using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataExtensionsSample.DataTypes;
using DataExtensionsSample.Helpers;
using DataExtensionsSample.OutputTypes;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace DataExtensionsSample.SourceDataCookers
{
    public sealed class ProcessDataCooker
        : SampleSourceDataCooker
    {
        public const string Identifier = "Process";
        public static readonly DataCookerPath CookerPath = new DataCookerPath(CustomSourceParser.SourceId, Identifier);

        private static class ProcessDataKeys
        {
            public const string Load = "ProcessLoad";
            public const string Unload = "ProcessUnload";
        }

        private static readonly HashSet<string> dataKeys = new HashSet<string>
        {
            ProcessDataKeys.Load,
            ProcessDataKeys.Unload,
        };

        private readonly HashSet<ProcessActivity> processActivity = new HashSet<ProcessActivity>();
        private readonly List<ProcessActivity> activeProcesses = new List<ProcessActivity>();
        private ProcessInfo processInfo;

        public ProcessDataCooker()
            : base(Identifier)
        {
        }

        [DataOutput]
        public ProcessInfo ProcessInfo => this.processInfo;

        public override string Description => "Provides data about processes.";

        /// <summary>
        /// This cooker needs to see only Load and Unload events. No "Options" property is specified, so
        /// only events whose Key is in this HashSet are received.
        /// </summary>
        public override ReadOnlyHashSet<string> DataKeys => new ReadOnlyHashSet<string>(dataKeys);

        /// <summary>
        /// This is called for each data element this data cooker has registered to receive.
        /// </summary>
        public override DataProcessingResult CookDataElement(
            SampleEvent data,
            ISampleEventContext context,
            CancellationToken cancellationToken)
        {
            switch(data.Name)
            {
                case ProcessDataKeys.Load:
                    var processLoad = JsonBinary.ToObject<ProcessLoad>(data.Value);
                    if(this.activeProcesses.Any(x => x.ProcessId == processLoad.Id))
                    {
                        // If an existing process has been found with the same id and is already running, this is a problem
                        return DataProcessingResult.CorruptData;
                    }
                    this.activeProcesses.Add(new ProcessActivity() { ProcessId = processLoad.Id, ProcessName = processLoad.Name, StartTime = data.Timestamp });
                    return DataProcessingResult.Processed;

                case ProcessDataKeys.Unload:
                    var processUnload = JsonBinary.ToObject<ProcessUnload>(data.Value);
                    int index = this.activeProcesses.FindIndex(p => p.ProcessId == processUnload.Id);

                    if(index == -1)
                    {
                        // This process started before this data set started recording

                        this.processActivity.Add(
                            new ProcessActivity
                            {
                                ProcessId = processUnload.Id,
                                ProcessName = processUnload.Name,
                                StartTime = Timestamp.Zero,
                                StopTime = data.Timestamp
                            });
                    }
                    else
                    {
                        var process = this.activeProcesses[index];
                        this.activeProcesses.RemoveAt(index);

                        process.StopTime = data.Timestamp;
                        this.processActivity.Add(process);
                    }
                    return DataProcessingResult.Processed;
            }

            return DataProcessingResult.Ignored;
        }

        public override void EndDataCooking(CancellationToken cancellationToken)
        {
            for(int x = 0; x < this.activeProcesses.Count; x++)
            {
                var process = this.activeProcesses[x];
                process.StopTime = Timestamp.MaxValue;
                this.processActivity.Add(process);
            }

            this.activeProcesses.Clear();

            this.processInfo = new ProcessInfo(this.processActivity);
        }
    }
}