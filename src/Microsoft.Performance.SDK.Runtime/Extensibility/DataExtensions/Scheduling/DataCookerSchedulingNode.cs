// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling
{
    internal enum SchedulingStatus
    {
        /// <summary>
        ///     The DataCookerSchedulingNode has not yet been scheduled.
        /// </summary>
        NotScheduled,

        /// <summary>
        ///     The DataCookerSchedulingNode is currently being scheduled.
        /// </summary>
        Scheduling,

        /// <summary>
        ///     The DataCookerSchedulingNode has completed scheduling.
        /// </summary>
        Scheduled
    }

    /// <summary>
    ///     This class is maintains scheduling state of a data cooker for use by
    ///     <see cref="SourceDataCookerScheduler"/>.
    ///     It also schedules the data cooker, setting the <see cref="PassIndex"/> and
    ///     <see cref="PassBlockIndex"/> values accordingly.
    /// </summary>
    internal class DataCookerSchedulingNode
    {
        internal static DataCookerSchedulingNode CreateSchedulingNode(IDataCookerDescriptor dataCooker)
        {
            Guard.NotNull(dataCooker, nameof(dataCooker));

            if (!(dataCooker is ISourceDataCookerDependencyTypes sourceDataCooker))
            {
                throw new ArgumentException(
                    $"Data Cooker {dataCooker.Path} does not support " +
                        $"{nameof(ISourceDataCookerDependencyTypes)}.",
                    nameof(dataCooker));
            }

            if (sourceDataCooker.DataProductionStrategy == DataProductionStrategy.AsRequired)
            {
                return new AsRequiredCookerSchedulingNode(dataCooker, sourceDataCooker);
            }

            return new DataCookerSchedulingNode(dataCooker, sourceDataCooker);
        }

        internal DataCookerSchedulingNode(
            IDataCookerDescriptor dataCooker,
            ISourceDataCookerDependencyTypes sourceDataCooker)
        {
            this.DataCooker = dataCooker;
            this.CookerDependencies = sourceDataCooker;
            this.SourceDataCookerDescriptor = this.CookerDependencies;
        }

        /// <summary>
        ///     Gets the data cooker represented by this node.
        /// </summary>
        internal IDataCookerDescriptor DataCooker { get; }

        /// <summary>
        ///     Gets cookers that the data cooker represented by this node relies on.
        /// </summary>
        internal ISourceDataCookerDependencyTypes CookerDependencies { get; }

        /// <summary>
        ///     Gets additional information about <see cref="DataCooker"/> used for scheduling.
        /// </summary>
        internal ISourceDataCookerDescriptor SourceDataCookerDescriptor { get; }

        /// <summary>
        ///     Gets the current state of this scheduling node.
        /// </summary>
        internal SchedulingStatus Status { get; private set; } = SchedulingStatus.NotScheduled;

        /// <summary>
        ///     Gets the data cooker path for <see cref="DataCooker"/>
        /// </summary>
        internal DataCookerPath DataCookerPath => this.DataCooker.Path;

        /// <summary>
        /// Gets the pass number through the data source in which <see cref="DataCooker"/> will participate.
        /// </summary>
        protected int PassIndex => this.Pass.Index;

        protected SchedulingPass Pass { get; set; }

        protected SchedulingBlock Block { get; set; }

        protected void SetStatus(SchedulingStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        ///     Data cookers within a data source pass are ordered - chunked into blocks. Within a given block, there
        ///     is no ordering, but the blocks themselves are ordered.
        ///     This is the index assigned to <see cref="DataCooker"/>, indicating when it will receive the data source
        ///     element for processing, with relation to other blocks in the same pass.
        /// </summary>
        protected int PassBlockIndex 
        {
            get
            {
                if (this.Block == null)
                {
                    Debugger.Break();
                }

                return this.Block.Index;
            }
        }

        /// <summary>
        ///     Assigns <see cref="PassIndex"/> and <see cref="PassBlockIndex"/>.
        /// </summary>
        /// <param name="scheduler">
        ///     The scheduler in which this node participates.
        /// </param>
        /// <param name="dependentPass">
        ///     The pass in which the dependent cooker that resulted in this call to <see cref="Schedule"/> runs.
        ///     This parameter may be null.
        /// </param>
        internal virtual void Schedule(ISourceDataCookerScheduler scheduler, SchedulingPass dependentPass)
        {
            Debug.Assert(!(scheduler is null));

            if (this.Status == SchedulingStatus.Scheduled)
            {
                return;
            }

            if (this.Status == SchedulingStatus.Scheduling)
            {
                throw new InvalidOperationException(
                    $"Source data cooker {nameof(this.DataCookerPath)} is involved in a circular dependency.");
            }

            this.Status = SchedulingStatus.Scheduling;

            // An AsRequired node will establish this before scheduling
            if (this.Pass == null)
            {
                this.Pass = scheduler.Pass0;
            }

            this.Block = this.Pass.Block0;

            var requiredCookerPaths = this.CookerDependencies.RequiredDataCookers ?? new List<DataCookerPath>();

            List<DataCookerSchedulingNode> dependencies;
            {
                // Sort dependencies so that AsRequired are done last. These need to know the final PassIndex
                // of this cooker before they can be scheduled.
                //

                var normalDependencies = new List<DataCookerSchedulingNode>();
                var asRequiredDependencies = new List<DataCookerSchedulingNode>();

                foreach (var requiredDataCookerPath in requiredCookerPaths)
                {
                    var sourceParserId = requiredDataCookerPath.SourceParserId;
                    if (!StringComparer.Ordinal.Equals(sourceParserId, this.DataCooker.Path.SourceParserId))
                    {
                        throw new InvalidOperationException(
                            $"A source data parser may not require data cookers from other source parsers: {this.DataCookerPath}");
                    }

                    var requiredCookerNode = scheduler.GetSchedulingNode(requiredDataCookerPath);
                    Debug.Assert(!(requiredCookerNode is null));

                    var productionStrategy = requiredCookerNode.SourceDataCookerDescriptor.DataProductionStrategy;
                    if (productionStrategy == DataProductionStrategy.AsRequired)
                    {
                        asRequiredDependencies.Add(requiredCookerNode);
                    }
                    else
                    {
                        normalDependencies.Add(requiredCookerNode);
                    }
                }

                if (this.SourceDataCookerDescriptor.DataProductionStrategy == DataProductionStrategy.AsRequired)
                {
                    if (normalDependencies.Any())
                    {
                        throw new InvalidOperationException(
                            $"A source data cooker with a {nameof(DataProductionStrategy)} " + 
                            $"of {nameof(DataProductionStrategy.AsRequired)} cannot depend on source data " +
                            $"cookers not using this same {nameof(DataProductionStrategy)}: {this.DataCooker.Path} " + 
                            $"depends on {normalDependencies[0].DataCookerPath}");
                    }
                }

                dependencies = normalDependencies.Concat(asRequiredDependencies).ToList();
            }

            // Make sure all of the dependencies are scheduled.
            foreach (var requiredCookerNode in dependencies)
            {
                requiredCookerNode.Schedule(scheduler, this.Pass);

                if (this.PassIndex > requiredCookerNode.PassIndex)
                {
                    continue;
                }

                if (this.CookerDependencies.DependencyTypes is null ||
                    !this.CookerDependencies.DependencyTypes.TryGetValue(
                        requiredCookerNode.DataCookerPath, out var dependencyType))
                {
                    dependencyType = DataCookerDependencyType.AlignedWithProductionStrategy;
                }

                var productionStrategy = requiredCookerNode.SourceDataCookerDescriptor.DataProductionStrategy;

                if (productionStrategy == DataProductionStrategy.AsRequired)
                {
                    if (dependencyType != DataCookerDependencyType.AlignedWithProductionStrategy)
                    {
                        // These source cookers will automatically run in any/all stages where a dependent source
                        // cooker is scheduled. Given that, only DataCookerDependencyType.AlignedWithProductionStrategy
                        // make sense.
                        //

                        throw new InvalidOperationException(
                            $"A SourceCooker whose {nameof(DataProductionStrategy)} is " +
                            $"{nameof(DataProductionStrategy.AsRequired)} can only be consumed using " +
                            $"{nameof(DataCookerDependencyType.AlignedWithProductionStrategy)}");
                    }
                }

                var oldPassIndex = this.Pass.Index;
                if (requiredCookerNode.PassIndex > this.PassIndex)
                {
                    this.Pass = requiredCookerNode.Pass;
                }

                if (productionStrategy == DataProductionStrategy.PostSourceParsing)
                {
                    if (dependencyType == DataCookerDependencyType.AlignedWithProductionStrategy)
                    {
                        if (requiredCookerNode.PassIndex == int.MaxValue)
                        {
                            throw new InvalidOperationException(
                                "The number of required passes through the data source exceeds maximum allowed value.");
                        }

                        if (requiredCookerNode.Pass.Next == null)
                        {
                            requiredCookerNode.Pass.CreateNext();
                        }
                        Debug.Assert(requiredCookerNode.Pass.Next != null);

                        // this cannot take place in the same pass as the required data cooker
                        if (requiredCookerNode.Pass.Next.Index > this.PassIndex)
                        {
                            this.Pass = requiredCookerNode.Pass.Next;
                        }
                    }
                }

                if (this.PassIndex > oldPassIndex)
                {
                    // If we've moved into a higher source pass, then the block
                    // should reset to zero, or we could end up in an invalid state.
                    //
                    this.Block = this.Pass.Block0;
                }

                if (this.PassIndex == requiredCookerNode.PassIndex)
                {
                    // This should be set at least to the requiredCooker, in case that cooker has a
                    // required cooker.
                    //
                    if (this.PassBlockIndex < requiredCookerNode.PassBlockIndex)
                    {
                        this.Block = requiredCookerNode.Block;
                    }

                    if (dependencyType == DataCookerDependencyType.AsConsumed ||
                        productionStrategy == DataProductionStrategy.AsRequired ||
                        (dependencyType == DataCookerDependencyType.AlignedWithProductionStrategy &&
                         productionStrategy == DataProductionStrategy.AsConsumed))
                    {
                        // if this takes place in the same pass as the required cooker, just make it come after
                        // the required cooker in the pass
                        //
                        if (requiredCookerNode.Block.Next == null)
                        {
                            this.Block = requiredCookerNode.Block.CreateNext();
                        }
                        else if (this.PassBlockIndex < requiredCookerNode.Block.Next.Index)
                        {
                            this.Block = requiredCookerNode.Block.Next;
                        }
                    }
                }
            }

            this.Block.Nodes.Add(this);
            this.Status = SchedulingStatus.Scheduled;
        }
    }

    internal class AsRequiredCookerSchedulingNode
        : DataCookerSchedulingNode
    {
        internal AsRequiredCookerSchedulingNode(
            IDataCookerDescriptor dataCooker,
            ISourceDataCookerDependencyTypes sourceDataCooker)
            : base(dataCooker, sourceDataCooker)
        {
        }

        // The Block will always be the same, as this can only depend on other AsRequired cookers.
        // But, it may exist in multiple Passes.
        //
        internal HashSet<SchedulingPass> Passes { get; } = new HashSet<SchedulingPass>();

        /// <summary>
        ///     Assigns the data cooker to the appropriate pass(es) and block(s).
        /// </summary>
        /// <param name="scheduler">
        ///     The scheduler in which this node participates.
        /// </param>
        /// <param name="dependentPass">
        ///     The pass in which the dependent cooker that resulted in this call to <see cref="Schedule"/> runs.
        ///     This parameter may be null.
        /// </param>
        internal override void Schedule(ISourceDataCookerScheduler scheduler, SchedulingPass dependentPass)
        {
            if (dependentPass == null)
            {
                // An AsRequired cooker should never be scheduled by itself - but only when required by
                // another cooker.
                //
                return;
            }

            if (this.Passes.Contains(dependentPass))
            {
                // Already been scheduled for this pass
                return;
            }

            // change the starting block to match the block of the dependent cooker
            this.Pass = dependentPass;
            this.SetStatus(SchedulingStatus.NotScheduled);
            base.Schedule(scheduler, null);

            Debug.Assert(this.Status == SchedulingStatus.Scheduled);
            Debug.Assert(this.Pass != null);
            Debug.Assert(this.Block != null);

            this.Passes.Add(dependentPass);
        }
    }
}
