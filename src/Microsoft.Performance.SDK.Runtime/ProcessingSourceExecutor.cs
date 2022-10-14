// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Executes the <see cref="ICustomDataProcessor"/> of a <see cref="IProcessingSource"/>.
    /// </summary>
    public sealed class ProcessingSourceExecutor
    {
        private List<TableDescriptor> enabledTables;
        private Dictionary<TableDescriptor, Exception> failedToEnableTables;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceExecutor"/>
        ///     class.
        /// </summary>
        public ProcessingSourceExecutor()
            : this(Logger.Create<ProcessingSourceExecutor>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceExecutor"/>
        ///     class.
        /// </summary>
        /// <param name="logger">
        ///     Logs messages about the state of executing a processing source.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceExecutor(ILogger logger)
        {
            Guard.NotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        ///     The custom data processor created by this object.
        /// </summary>
        public ICustomDataProcessor Processor { get; private set; }

        /// <summary>
        ///     Provides access to the <see cref="ExecutionContext"/> used to initialize this object.
        /// </summary>
        public ExecutionContext Context { get; private set; }

        /// <summary>
        ///     Creates the custom data processor and enables the specified tables.
        /// </summary>
        /// <param name="context">
        ///     Context about the processor and tables to enable.
        /// </param>
        public void InitializeCustomDataProcessor(ExecutionContext context)
        {
            Guard.NotNull(context, nameof(context));

            this.Processor = context.ProcessingSource.CreateProcessor(
                context.DataSourceGroup,
                context.ProcessorEnvironment,
                context.CommandLineOptions);

            if (this.Processor == null)
            {
                throw new InvalidOperationException("Unable to create the data processor.");
            }

            this.Context = context;

            this.enabledTables = new List<TableDescriptor>();
            this.failedToEnableTables = new Dictionary<TableDescriptor, Exception>();
            Parallel.ForEach(
                context.TablesToEnable,
                table =>
                {
                    try
                    {
                        this.Processor.EnableTable(table);
                        lock (this.enabledTables)
                        {
                            this.enabledTables.Add(table);
                        }
                    }
                    catch (Exception e)
                    {
                        lock (this.failedToEnableTables)
                        {
                            this.failedToEnableTables[table] = e;
                            this.logger.Warn("Unable to enable {0}: {1}", table, e);
                        }
                    }
                });
        }

        /// <summary>
        ///     Runs the custom data processor and builds enabled tables.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Cancellation token.
        /// </param>
        /// <returns>
        ///     Process execution and table build result context.
        /// </returns>
        public async Task<ExecutionResult> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            if (this.Processor == null)
            {
                throw new InvalidOperationException($"{nameof(this.InitializeCustomDataProcessor)} wasn't successfully called before calling this method.");
            }

            Debug.Assert(this.enabledTables != null, $"{nameof(this.enabledTables)} is somehow null, but the processor was created successfully.");
            Debug.Assert(this.failedToEnableTables != null, $"{nameof(this.failedToEnableTables)} is somehow null, but the processor was created successfully.");

            try
            {
                await this.Processor.ProcessAsync(this.Context.ProgressReporter, cancellationToken);
            }
            catch (Exception e)
            {
                return new ExecutionResult(this.Context, this.Processor, e);
            }

            DataSourceInfo info;
            Exception infoFailure;
            try
            {
                info = this.Processor.GetDataSourceInfo() ?? DataSourceInfo.Default;
                infoFailure = null;
            }
            catch (Exception e)
            {
                info = DataSourceInfo.Default;
                infoFailure = e;
                this.logger.Warn("Unable to get data source info for {0}: {1}", this.Processor, e);
            }

            var metadataName = this.Context.ProcessingSource.Name;

            return new ExecutionResult(
                this.Context,
                info,
                infoFailure,
                this.Processor,
                this.failedToEnableTables,
                metadataName);
        }
    }
}
