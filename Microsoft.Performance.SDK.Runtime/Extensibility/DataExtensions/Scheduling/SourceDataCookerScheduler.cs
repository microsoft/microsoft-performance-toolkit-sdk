// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling
{
    /// <summary>
    ///     This clusters a set of source data cookers all targeting the same source parser into
    ///     a set of source passes, and orders them within each pass to meet the dependency requirements
    ///     specified by each source data parser.
    ///
    ///     Note that each source pass will require the entire source to be parsed, increasing the time
    ///     for processing the source.
    /// </summary>
    internal class SourceDataCookerScheduler
        : ISourceDataCookerScheduler
    {
        private readonly LinkedList<SchedulingPass> passes;

        private readonly Dictionary<DataCookerPath, IDataCookerDescriptor> dataCookerPathsToDataCookers;
        private readonly Dictionary<IDataCookerDescriptor, DataCookerSchedulingNode> cookersToNodes;

        private bool scheduled = false;

        internal SourceDataCookerScheduler(string sourceParserId)
        {
            Guard.NotNullOrWhiteSpace(sourceParserId, nameof(sourceParserId));

            this.SourceParserId = sourceParserId;

            this.passes = new LinkedList<SchedulingPass>();
            SchedulingPass.CreatePass(0, this.passes);

            this.dataCookerPathsToDataCookers = new Dictionary<DataCookerPath, IDataCookerDescriptor>();
            this.cookersToNodes = new Dictionary<IDataCookerDescriptor, DataCookerSchedulingNode>();

            this.DataCookersBySourcePass = new List<List<IDataCookerDescriptor>>();
        }

        public SchedulingPass Pass0 => this.passes.First.Value;

        public DataCookerSchedulingNode GetSchedulingNode(
            DataCookerPath dataCookerPath)
        {
            if (this.dataCookerPathsToDataCookers.TryGetValue(dataCookerPath, out var dataCooker))
            {
                if (this.cookersToNodes.TryGetValue(dataCooker, out var schedulingNode))
                {
                    return schedulingNode;
                }
            }

            // it's a bug if we've reached this
            // cookers that don't have all their requirements met shouldn't be enabled on a source session.
            //
            throw new InvalidOperationException(
                $"A required cooker is not available for {dataCookerPath}");
        }

        internal string SourceParserId { get; }

        internal List<List<IDataCookerDescriptor>> DataCookersBySourcePass { get; private set; }

        internal void ScheduleDataCookers(IReadOnlyCollection<IDataCookerDescriptor> dataCookers)
        {
            Guard.NotNull(dataCookers, nameof(dataCookers));

            if (this.scheduled)
            {
                throw new InvalidOperationException(
                    $"{nameof(this.ScheduleDataCookers)} may not be called more than once.");
            }

            this.scheduled = true;

            foreach (var dataCooker in dataCookers)
            {
                if (!StringComparer.Ordinal.Equals(dataCooker.Path.SourceParserId, this.SourceParserId))
                {
                    throw new ArgumentException(
                        $"Data Cooker {dataCooker.Path} does not target source parser {this.SourceParserId}.",
                        nameof(dataCooker));
                }

                if (!(dataCooker is ISourceDataCookerDescriptor))
                {
                    throw new ArgumentException(
                        $"Cooker {dataCooker.Path} is not a source data cooker.",
                        nameof(dataCookers));
                }

                this.dataCookerPathsToDataCookers.Add(dataCooker.Path, dataCooker);
                this.cookersToNodes.Add(dataCooker, DataCookerSchedulingNode.CreateSchedulingNode(dataCooker));
            }

            // Make sure each cooker is scheduled and determine the number of passes, blocks, and cookers
            // per block.
            //
            foreach (var kvp in this.cookersToNodes)
            {
                IDataCookerDescriptor dataCooker = kvp.Key;
                var sourceCooker = dataCooker as ISourceDataCookerDescriptor;
                var schedulingNode = kvp.Value;

                Debug.Assert(!(sourceCooker is null));
                Debug.Assert(schedulingNode.Status != SchedulingStatus.Scheduling);

                schedulingNode.Schedule(this, null);
            }

            this.DataCookersBySourcePass = new List<List<IDataCookerDescriptor>>(this.passes.Count);
            foreach (var scheduledPass in this.passes)
            {
                this.DataCookersBySourcePass.Add(scheduledPass.GetCookers());
            }
        }
    }
}
