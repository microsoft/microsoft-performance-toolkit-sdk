using System.Linq;
using System.Collections.Generic;
using Microsoft.Performance.SDK;

namespace DataExtensionsSample.OutputTypes
{
    public sealed class ProcessInfo
    {
        private List<ProcessActivity> processActivities;
        private Dictionary<uint, List<int>> processesById = new Dictionary<uint, List<int>>();

        internal ProcessInfo(HashSet<ProcessActivity> processActivity)
        {
            this.processActivities = processActivity.ToList();
            this.ProcessActivity = this.processActivities.AsReadOnly();

            // Note: if implementing this for a source that may expose a lot of data, consider doing
            // this on a background thread. Because this is a data extension framework sample, and there
            // isn't a lot of data, we're just processing this data in the constructor.
            //

            ProcessById();
        }

        public IReadOnlyList<ProcessActivity> ProcessActivity { get; }

        public ProcessActivity? FindProcess(uint processId, Timestamp time)
        {
            if(!this.processesById.TryGetValue(processId, out var processes))
            {
                return null;
            }

            foreach(var processIndex in processes)
            {
                var process = this.processActivities[processIndex];
                if(process.StartTime <= time)
                {
                    if(process.StopTime >= time)
                    {
                        return process;
                    }
                }
            }

            return null;
        }

        private void ProcessById()
        {
            for(int x = 0; x < this.processActivities.Count; x++)
            {
                var process = this.processActivities[x];

                if(this.processesById.TryGetValue(process.ProcessId, out var processes))
                {
                    processes.Add(x);
                    continue;
                }

                processes = new List<int>() { x };
                this.processesById.Add(process.ProcessId, processes);
            }
        }
    }
}