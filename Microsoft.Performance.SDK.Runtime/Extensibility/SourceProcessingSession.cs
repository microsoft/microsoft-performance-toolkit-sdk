// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <inheritdoc />
    /// <summary>
    ///     This class adds multiple passes through the source parser to the base class, when necessary. The number of
    ///     passes required is determined by the requirements and processing strategies of the registered source data
    ///     cookers.
    /// </summary>
    /// <typeparam name="T">
    ///     Data element type.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     Data context type.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Data element key type.
    /// </typeparam>
    internal class SourceProcessingSession<T, TContext, TKey>
        : BaseSourceProcessingSession<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        private static readonly int InvalidPass = -1;

        private readonly SourceDataCookerScheduler scheduler;

        private int currentPassIndex = InvalidPass;
        private List<List<ISourceDataCooker<T, TContext, TKey>>> sourceCookersByPass;

        private int maxSourcePassCount;

        internal SourceProcessingSession(ISourceParser<T, TContext, TKey> sourceParser)
            : this(sourceParser, EqualityComparer<TKey>.Default)
        {
        }

        internal SourceProcessingSession(
            ISourceParser<T, TContext, TKey> sourceParser,
            IEqualityComparer<TKey> comparer)
            : base(sourceParser, comparer)
        {
            Debug.Assert(sourceParser != null, nameof(sourceParser));

            this.scheduler = new SourceDataCookerScheduler(sourceParser.Id);
            this.maxSourcePassCount = sourceParser.MaxSourceParseCount;
        }

        /// <inheritdoc />
        public override void ProcessSource(ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(progress, nameof(progress));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            int countOfPassesToProcess = this.ScheduleSourceDataCookers();

            // Special case 0 registered cookers. This can happen if a CDS supports data extensions, but no data extensions
            // are present. In this case, we still need to process the source, to handle any possible "internal"
            // data processors.
            //
            if (countOfPassesToProcess == 0)
            {
                this.currentPassIndex = 0;
                this.InitializeForSourceParsing();
                this.SourceParser.ProcessSource(this, logger, progress, cancellationToken);
                this.currentPassIndex = InvalidPass;
                return;
            }

            for (int passIndex = 0; passIndex < countOfPassesToProcess; passIndex++)
            {
                var sourceDataCookers = this.sourceCookersByPass[passIndex];

                foreach (var sourceDataCooker in sourceDataCookers)
                {
                    IReadOnlyCollection<DataCookerPath> availableCookers = sourceDataCooker.RequiredDataCookers;
                    if (availableCookers is null)
                    {
                        availableCookers = new List<DataCookerPath>().AsReadOnly();
                    }

                    var dependencyData = new SourceDataCookerDependencyProvider<T, TContext, TKey>(
                            new HashSet<DataCookerPath>(availableCookers), this);

                    sourceDataCooker.BeginDataCooking(dependencyData, cancellationToken);
                }
            }

            for (int passIndex = 0; passIndex < countOfPassesToProcess; passIndex++)
            {
                this.currentPassIndex = passIndex;

                this.InitializeForSourceParsing();

                // todo:we'll need a different IProgress sent in when there are multiple passes
                this.SourceParser.ProcessSource(this, logger, progress, cancellationToken);

                foreach (var cooker in this.sourceCookersByPass[passIndex])
                {
                    cooker.EndDataCooking(cancellationToken);
                }
            }

            this.currentPassIndex = InvalidPass;
        }

        protected override void SetActiveDataCookers()
        {
            var currentCookers = this.sourceCookersByPass[this.currentPassIndex];

            ClearActiveCookers(currentCookers.Count);
            foreach (var sourceDataCooker in currentCookers)
            {
                ActivateCooker(sourceDataCooker);
            }
        }

        /// <summary>
        ///     This method determines in which pass through the source each registered data cooker should participate.
        /// </summary>
        /// <returns>
        ///     The required number of passes through the source.
        /// </returns>
        private int ScheduleSourceDataCookers()
        {
            if (this.RegisteredCookers.Count == 0)
            {
                this.sourceCookersByPass = new List<List<ISourceDataCooker<T, TContext, TKey>>>(1)
                {
                    new List<ISourceDataCooker<T, TContext, TKey>>(0)
                };
                return 0;
            }

            scheduler.ScheduleDataCookers(this.RegisteredCookers);

            var cookersByPass = this.scheduler.DataCookersBySourcePass;

            if (this.maxSourcePassCount == 0)
            {
                this.maxSourcePassCount = cookersByPass.Count;
            }

            Debug.Assert(cookersByPass.Count <= this.maxSourcePassCount);

            for (int passIndex = this.maxSourcePassCount;
                passIndex < cookersByPass.Count;
                passIndex++)
            {
                foreach (var cooker in cookersByPass[passIndex])
                {
                    Console.Error.WriteLine(
                        $"Source data cooker {cooker.Path} is disabled as it would require too many passes through the source.");
                }
            }

            int countOfPassesToProcess = Math.Min(cookersByPass.Count, this.maxSourcePassCount);
            this.sourceCookersByPass = new List<List<ISourceDataCooker<T, TContext, TKey>>>(countOfPassesToProcess);

            for (int passIndex = 0; passIndex < countOfPassesToProcess; passIndex++)
            {
                var cookers = cookersByPass[passIndex];
                this.sourceCookersByPass.Add(new List<ISourceDataCooker<T, TContext, TKey>>(cookers.Count));

                foreach (var cooker in cookers)
                {
                    if (cooker is ISourceDataCooker<T, TContext, TKey> sourceDataCooker)
                    {
                        this.sourceCookersByPass[passIndex].Add(sourceDataCooker);
                    }
                }
            }

            return countOfPassesToProcess;
        }
    }
}
