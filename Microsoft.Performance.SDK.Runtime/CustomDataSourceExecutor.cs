// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Executes the processor of a data source.
    /// </summary>
    public sealed class CustomDataSourceExecutor
    {
        private List<TableDescriptor> enabledTables;
        private Dictionary<TableDescriptor, Exception> failedToEnableTables;

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

            this.Processor = context.CustomDataSource.CreateProcessor(
                context.DataSources,
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
                            Console.Error.WriteLine("Unable to enable {0}: {1}", table, e);
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

            //_CDS_ Should we "delayload" metadata tables?
            var metadataTables = new List<MetadataTableBuilder>();
            Exception metadataFailure;
            try
            {
                var factory = new MetadataTableBuilderFactory();
                this.Processor.BuildMetadataTables(factory);
                foreach (var table in factory.CreatedTables)
                {
                    metadataTables.Add(table);
                }

                metadataFailure = null;
            }
            catch (Exception e)
            {
                metadataTables.Clear();
                metadataFailure = e;
                Console.Error.WriteLine("Unable to build metadata tables for {0}: {1}", this.Processor, e);
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
                Console.Error.WriteLine("Unable to get data source info for {0}: {1}", this.Processor, e);
            }

            var metadataName = this.Context.CustomDataSource.TryGetName();

            return new ExecutionResult(
                this.Context,
                info,
                infoFailure,
                this.Processor,
                this.failedToEnableTables,
                metadataName,
                metadataTables,
                metadataFailure);
        }
    }
}
