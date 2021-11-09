// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a base class for implementing custom data processors that
    ///     simplifies some of the management of tables and processing.
    ///     This class is meant to be used in conjunction with <see cref="ProcessingSource"/>.
    /// </summary>
    public abstract class CustomDataProcessor
        : ICustomDataProcessor,
          IDataDerivedTables
    {
        private readonly HashSet<TableDescriptor> enabledTables = new HashSet<TableDescriptor>();

        private readonly Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>
            dataDerivedTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataProcessor"/>
        ///     class.
        /// </summary>
        protected CustomDataProcessor(
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment,
            IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(applicationEnvironment, nameof(applicationEnvironment));
            Guard.NotNull(allTablesMapping, nameof(allTablesMapping));
            Guard.NotNull(processorEnvironment, nameof(processorEnvironment));

            this.Logger = processorEnvironment.CreateLogger(this.GetType());

            this.ApplicationEnvironment = applicationEnvironment;
            this.ProcessorEnvironment = processorEnvironment;
            this.EnabledTables = new ReadOnlyHashSet<TableDescriptor>(this.enabledTables);
            this.Options = options;
            this.TableDescriptorToBuildAction = allTablesMapping;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<TableDescriptor> DataDerivedTables => this.dataDerivedTables.Keys;

        /// <summary>
        ///     Gets the <see cref="IApplicationEnvironment"/> for this processor.
        /// </summary>
        protected IApplicationEnvironment ApplicationEnvironment { get; }

        /// <summary>
        ///     Gets the session services available for this processor.
        /// </summary>
        protected IProcessorEnvironment ProcessorEnvironment { get; }

        /// <summary>
        ///     Gets a collection of tables that have been enabled for this processor.
        /// </summary>
        protected ReadOnlyHashSet<TableDescriptor> EnabledTables { get; }

        /// <summary>
        ///     Gets the command line options that have been passed to this processor.
        /// </summary>
        protected ProcessorOptions Options { get; }

        /// <summary>
        ///     Gets a mapping of table descriptors to their resolved table builder actions.
        /// </summary>
        protected IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> TableDescriptorToBuildAction { get; }

        /// <summary>
        ///     Used to log data specific to this data processor.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        public void BuildMetadataTables(IMetadataTableBuilderFactory metadataTableBuilderFactory)
        {
            Guard.NotNull(metadataTableBuilderFactory, nameof(metadataTableBuilderFactory));

            foreach (var kvp in this.TableDescriptorToBuildAction.Where(x => x.Key.IsMetadataTable && !x.Key.RequiresDataExtensions()))
            {
                var builder = metadataTableBuilderFactory.Create(kvp.Key);
                Debug.Assert(builder != null);

                this.BuildTableCore(kvp.Key, kvp.Value, builder);
            }
        }

        /// <inheritdoc />
        public void EnableTable(TableDescriptor tableDescriptor)
        {
            lock (this.enabledTables)
            {
                if (this.enabledTables.Contains(tableDescriptor))
                {
                    return;
                }

                OnBeforeEnableTable(tableDescriptor);
                this.enabledTables.Add(tableDescriptor);
                OnAfterEnableTable(tableDescriptor);
            }
        }

        /// <inheritdoc />
        public bool TryEnableTable(TableDescriptor tableDescriptor)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            try
            {
                EnableTable(tableDescriptor);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <inheritdoc />
        public void BuildTable(TableDescriptor table, ITableBuilder tableBuilder)
        {
            Guard.NotNull(table, nameof(table));
            Guard.NotNull(tableBuilder, nameof(tableBuilder));

            if (this.TableDescriptorToBuildAction.TryGetValue(table, out var buildTable))
            {
                this.BuildTableCore(table, buildTable, tableBuilder);
            }
            else if (this.dataDerivedTables.TryGetValue(table, out buildTable))
            {
                this.BuildTableCore(table, buildTable, tableBuilder);
            }
            else
            {
                Debug.Assert(false, "A table was requested that is not known by the data source.");
                throw new InvalidOperationException(
                    "Table " + table + " was requested to be built but is not supported by " + this.GetType());
            }
        }

        /// <summary>
        ///     When overridden in a derived class, creates an instance of the service defined by the
        ///     given table's <see cref="TableDescriptor.ServiceInterface"/>.
        /// </summary>
        /// <param name="table">
        ///     The table whose service is to be created.
        /// </param>
        /// <returns>
        ///     The created service, if the table specifies a service; <c>null</c> otherwise.
        /// </returns>
        public virtual ITableService CreateTableService(TableDescriptor table)
        {
            if (table is null)
            {
                return null;
            }

            if (table.ServiceInterface is null)
            {
                return null;
            }

            if (table.ServiceInterface.IsPublic &&
                table.ServiceInterface.IsInstantiatable() &&
                table.ServiceInterface.Implements<ITableService>())
            {
                return (ITableService)Activator.CreateInstance(table.ServiceInterface);
            }

            return null;
        }

        /// <inheritdoc />
        public abstract DataSourceInfo GetDataSourceInfo();

        /// <inheritdoc />
        public virtual bool DoesTableHaveData(TableDescriptor table)
        {
            return true;
        }

        /// <inheritdoc />
        public Task ProcessAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            return ProcessAsyncCore(progress, cancellationToken);
        }

        /// <inheritdoc/>
        public IEnumerable<TableDescriptor> GetEnabledTables()
        {
            lock (this.enabledTables)
            {
                return this.enabledTables.ToList();
            }
        }

        /// <summary>
        ///     Enable metadata tables.
        /// </summary>
        /// <remarks>
        ///     The default implementation just adds the tables to <see cref="EnabledTables"/>.
        /// </remarks>
        /// <param name="metadataTables">
        ///     Metadata tables to process.
        /// </param>
        protected virtual void EnableMetadataTables(IEnumerable<TableDescriptor> metadataTables)
        {
            foreach (var table in metadataTables)
            {
                EnableTable(table);
            }
        }

        /// <summary>
        ///     Asynchronously processes the data source.
        /// </summary>
        /// <param name="progress">
        ///     Provides a method of updating the application as to this
        ///     processor's progress.
        /// </param>
        /// <param name="cancellationToken">
        ///     A means of the application signalling to the processor that
        ///     it should abort processing.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        protected abstract Task ProcessAsyncCore(
            IProgress<int> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///     When overridden in a derived class, builds the requested
        ///     table using the given <see cref="ITableBuilder"/>.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The descriptor of the requested table.
        /// </param>
        /// <param name="createTable">
        ///     Called to create the requested table.
        ///     The object parameter of the Action may be used by the
        ///     CustomDataProcessor to pass context.
        /// </param>
        /// <param name="tableBuilder">
        ///     The builder to use to build the table.
        /// </param>
        protected abstract void BuildTableCore(
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> createTable,
            ITableBuilder tableBuilder);

        /// <summary>
        ///     This is called before a table has been enabled on this data processor.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     Table that will be enabled.
        /// </param>
        protected virtual void OnBeforeEnableTable(TableDescriptor tableDescriptor)
        {
        }

        /// <summary>
        ///     This is called after a table has been enabled on this data processor. The default implementation does
        ///     nothing.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     Table that was enabled.
        /// </param>
        protected virtual void OnAfterEnableTable(TableDescriptor tableDescriptor)
        {
        }

        /// <summary>
        /// When a custom data processor needs to generate tables dynamically based on data content, call this
        /// method to register the table with the runtime.
        /// </summary>
        /// <param name="tableDescriptor">Descriptor for the generated table.</param>
        /// <param name="buildAction">Action called to create the requested table.</param>
        protected void AddTableGeneratedFromDataProcessing(
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> buildAction)
        {
            // the buildAction may be null because the custom data processor may have a special way to handle
            // these data derived tables - and that can be done through BuildTableCore
            //

            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            if (this.dataDerivedTables.ContainsKey(tableDescriptor))
            {
                throw new ArgumentException(
                    $"The data derived table already exists in the processor: {tableDescriptor}",
                    nameof(tableDescriptor));
            }

            if (this.TableDescriptorToBuildAction.ContainsKey(tableDescriptor))
            {
                throw new ArgumentException(
                    $"The data derived table already exists in the processor as a static table: {tableDescriptor}",
                    nameof(tableDescriptor));
            }

            this.dataDerivedTables.Add(tableDescriptor, buildAction);
        }
    }
}
