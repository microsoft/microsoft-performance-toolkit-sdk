// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents a container of the execution results from the <see cref="IProcessingSource"/>.
    /// </summary>
    public sealed class ExecutionResult
    {
        /// <summary>
        ///     Initializes a new Execution result for success case.
        /// </summary>
        /// <param name="context">
        ///     <see cref="ExecutionContext"/> from the processing state.
        /// </param>
        /// <param name="dataSourceInfo">
        ///     <see cref="DataSourceInfo"/> from the <see cref="IProcessingSource"/> being processed.
        /// </param>
        /// <param name="dataSourceInfoFailure">
        ///     <see cref="Exception"/> for the processing state (if available).
        /// </param>
        /// <param name="processor">
        ///     <see cref="ICustomDataProcessor"/> associated to the processing state.
        /// </param>
        /// <param name="enableFailures">
        ///     Dictionary representing the failures for <see cref="TableDescriptor"/> (if any).
        /// </param>
        /// <param name="metadataName">
        ///     Name of the <see cref="IProcessingSource"/> being processed.
        /// </param>
        public ExecutionResult(
            ExecutionContext context,
            DataSourceInfo dataSourceInfo,
            Exception dataSourceInfoFailure,
            ICustomDataProcessor processor,
            IDictionary<TableDescriptor, Exception> enableFailures,
            string metadataName)
            : this(enableFailures, context.TablesToEnable)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(dataSourceInfo, nameof(dataSourceInfo));
            Guard.NotNull(processor, nameof(processor));
            Guard.NotNullOrWhiteSpace(metadataName, nameof(metadataName));

            this.AssociatedWithProcessingSource = true;

            // Exception may be null.

            this.Context = context;
            this.Processor = processor;
            this.DataSourceInfo = dataSourceInfo;
            this.DataSourceInfoFailure = dataSourceInfoFailure;

            this.MetadataName = metadataName;

            Debug.Assert(this.RequestedTables != null);

            // If the processor contains tables that were generated while processing the data source, then add them to our RequestedTables.
            // todo: RequestedTables - should we rename this? Just make it AvailableTables or something similar?
            if (processor is IDataDerivedTables postProcessTables &&
                postProcessTables.DataDerivedTables != null)
            {
                // do some extra validation on these table descriptors, as they might have been generated by hand
                // rather than our usual library
                //

                foreach (var tableDescriptor in postProcessTables.DataDerivedTables)
                {
                    if (tableDescriptor.PrebuiltTableConfigurations is null)
                    {
                        var tableConfigurations = new TableConfigurations(tableDescriptor.Guid)
                        {
                            Configurations = Enumerable.Empty<TableConfiguration>()
                        };

                        tableDescriptor.PrebuiltTableConfigurations = tableConfigurations;
                    }
                }

                // combine these new tables with any existing requested tables for this processor
                this.RequestedTables = this.RequestedTables.Concat(postProcessTables.DataDerivedTables);
            }
        }

        /// <summary>
        ///     Initializes a new faulting ExecutionResult.
        /// </summary>
        /// <param name="context">
        ///     <see cref="ExecutionContext"/> from the processing state.
        /// </param>
        /// <param name="processor">
        ///     <see cref="ICustomDataProcessor"/> associated to the processing state.
        /// </param>
        /// <param name="processorFault">
        ///      Faulting <see cref="Exception"/>.
        /// </param>
        public ExecutionResult(
            ExecutionContext context,
            ICustomDataProcessor processor,
            Exception processorFault)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(processor, nameof(processor));
            Guard.NotNull(processorFault, nameof(processorFault));

            this.Context = context;
            this.DataSourceInfo = DataSourceInfo.None;
            this.Processor = processor;
            this.RequestedTables = context.TablesToEnable.ToList().AsReadOnly();
            this.ProcessorFault = processorFault;

            this.EnableFailures = new ReadOnlyDictionary<TableDescriptor, Exception>(new Dictionary<TableDescriptor, Exception>());
        }

        /// <summary>
        ///     Stores a set of basic data that is always expected to be set.
        /// </summary>
        /// <param name="enableFailures">Tables that failed to be enabled.</param>
        /// <param name="builtTables">Tables that were built.</param>
        /// <param name="buildFailures">Tables that failed to build.</param>
        public ExecutionResult(
            IDictionary<TableDescriptor, Exception> enableFailures,
            IEnumerable<TableDescriptor> requestedTables)
        {
            Guard.NotNull(enableFailures, nameof(enableFailures));

            this.EnableFailures = new ReadOnlyDictionary<TableDescriptor, Exception>(enableFailures);
            this.RequestedTables = requestedTables;
        }

        /// <summary>
        ///     Gets the tables that failed to enable.
        /// </summary>
        public IReadOnlyDictionary<TableDescriptor, Exception> EnableFailures { get; }

        /// <summary>
        ///     Gets the collection of all tables that were requested by the application.
        /// </summary>
        public IEnumerable<TableDescriptor> RequestedTables { get; }

        /// <summary>
        ///     When true, this object has additional data from an <see cref="IProcessingSource"/> and
        ///     <see cref="ICustomDataProcessor"/>.
        /// </summary>
        public bool AssociatedWithProcessingSource { get; }

        /// ---- The properties below are associated with a <see cref="IProcessingSource"/> and <see cref="ICustomDataProcessor"/> ----

        /// <summary>
        ///     Gets the context that produced this result.
        /// </summary>
        public ExecutionContext Context { get; }

        /// <summary>
        ///     Gets the processor that produced this result.
        /// </summary>
        public ICustomDataProcessor Processor { get; }

        /// <summary>
        ///     Gets the information about the data source (s).
        /// </summary>
        public DataSourceInfo DataSourceInfo { get; }

        /// <summary>
        ///     Gets the failure that occurred, if any, while trying to
        ///     retrieve information about the data source from the processor.
        ///     This property will be <c>null</c> if <see cref="DataSourceInfo"/> is
        ///     populated.
        /// </summary>
        public Exception DataSourceInfoFailure { get; }

        /// <summary>
        ///     Gets the name that describes the metadata.
        /// </summary>
        public string MetadataName { get; }

        /// <summary>
        ///     Gets a value indicating whether the processor itself faulted.
        ///     If the processor is faulted, then no tables will have been created.
        ///     Only <see cref="Context" />, <see cref="Processor"/>,
        ///     <see cref="RequestedTables" />, and <see cref="ProcessorFault" />
        ///     will be valid.
        /// </summary>
        public bool IsProcessorFaulted => this.ProcessorFault != null;

        /// <summary>
        ///     The <see cref="Exception" /> that caused the processor to fault.
        ///     If this is an <see cref="ExtensionException" />, then the processor
        ///     itself reported the condition. Any other exception type means an
        ///     unexpected error occurred, and the plugin author should probably
        ///     be notified of the issue.
        /// </summary>
        public Exception ProcessorFault { get; set; }
    }
}
