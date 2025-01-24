// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a base class for implementing <see cref="IProcessingSource"/>s that
    ///     contains default logic for discovering tables, providing their
    ///     descriptors.
    /// </summary>
    public abstract class ProcessingSource
        : IProcessingSource
    {
        private readonly HashSet<TableDescriptor> allTables;

        private readonly HashSet<TableDescriptor> metadataTables;
        private readonly ReadOnlyHashSet<TableDescriptor> metadataTablesRO;

        // todo: when the constructor that takes a table provider is removed in v2, this should 
        // be removed.
        private readonly IProcessingSourceTableProvider tableProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSource"/>
        ///     class.
        /// </summary>
        protected ProcessingSource()
        {
            this.allTables = new HashSet<TableDescriptor>();
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSource"/>
        ///     class.
        /// </summary>
        /// <param name="tableDiscoverer">
        ///     Object used to provide the tables that are exposed by this <see cref="ProcessingSource"/>.
        /// </param>
        /// <remarks>
        ///     All discovered tables will be associated with this <see cref="ProcessingSource"/>. When a consumer
        ///     requests the Engine to enable or build a <see cref="TableDescriptor"/> that is in this list, the
        ///     <see cref="TableDescriptor"/> will be passed into
        ///     <see cref="ICustomDataProcessor.EnableTable(TableDescriptor)"/> or
        ///     <see cref="ICustomDataProcessor.BuildTable(TableDescriptor, ITableBuilder)"/> for the 
        ///     <see cref="ICustomDataProcessor"/> returned from
        ///     <see cref="ProcessingSource.CreateProcessor(IDataSource, IProcessorEnvironment, ProcessorOptions)"/>
        ///     or
        ///     <see cref="ProcessingSource.CreateProcessor(IEnumerable{IDataSource}, IProcessorEnvironment, ProcessorOptions)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="tableDiscoverer"/> is <c>null</c>.
        /// </exception>
        [Obsolete("This constructor will be removed in version 2. Implement CreateTableProvider instead.", false)]
        protected ProcessingSource(IProcessingSourceTableProvider tableDiscoverer)
        {
            Guard.NotNull(tableDiscoverer, nameof(tableDiscoverer));

            this.allTables = new HashSet<TableDescriptor>();
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);

            this.tableProvider = tableDiscoverer;
        }

        /// <inheritdoc />
        public IEnumerable<TableDescriptor> DataTables => this.allTables.Except(this.metadataTablesRO);

        /// <inheritdoc />
        public IEnumerable<TableDescriptor> MetadataTables => this.metadataTablesRO;

        /// <inheritdoc />
        public virtual IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

        /// <inheritdoc />
        public virtual IEnumerable<PluginOption> PluginOptions => Enumerable.Empty<PluginOption>();

        /// <summary>
        ///     Gets a mapping of <see cref="TableDescriptor"/> to the concrete
        ///     <see cref="Type" /> of Table described by the descriptor. This
        ///     mapping includes the data and metadata tables.
        /// </summary>
        protected IEnumerable<TableDescriptor> AllTables => this.allTables.AsEnumerable();

        /// <summary>
        ///     This is set by default when the <see cref="SetApplicationEnvironment"/> is called by the runtime.
        ///     <seealso cref="IApplicationEnvironment"/>
        /// </summary>
        protected IApplicationEnvironment ApplicationEnvironment { get; private set; }

        /// <summary>
        ///     This is set when <see cref="SetLogger"/> is called by the runtime.
        ///     <seealso cref="ILogger"/>
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <inheritdoc />
        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
            this.ApplicationEnvironment = applicationEnvironment;

            // Call derived class in case things need to be set before calling InitializeAllTables
            SetApplicationEnvironmentCore(applicationEnvironment);

            this.InitializeAllTables(applicationEnvironment.Serializer);
        }

        /// <inheritdoc />
        public void SetLogger(ILogger logger)
        {
            this.Logger = logger;

            SetLoggerCore(logger);
        }

        /// <inheritdoc />
        public virtual ProcessingSourceInfo GetAboutInfo()
        {
            return new ProcessingSourceInfo
            {
                Owners = new[]
                {
                    new ContactInfo
                    {
                        Name = "<Your Name Here>",
                        Address = "<Your Address Here>",
                        EmailAddresses = new[]
                        {
                            "<your email addresses here>",
                        },
                        PhoneNumbers = new[]
                        {
                            "<your phone numbers here>",
                        },
                    },
                },
                ProjectInfo = new ProjectInfo
                {
                    Uri = "<project URI here>",
                },
                LicenseInfo = new LicenseInfo
                {
                    Name = "<License Name Here>",
                    Uri = "<License URI here>",
                    Text = "<Optional License Text Here>",
                },
                CopyrightNotice = "Copyright (C) " + DateTime.UtcNow.Year,
            };
        }

        /// <inheritdoc />
        public ICustomDataProcessor CreateProcessor(
            IDataSource dataSource,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            Guard.NotNull(dataSource, nameof(dataSource));

            return this.CreateProcessor(
                new[] { dataSource, },
                processorEnvironment,
                options);
        }

        /// <inheritdoc />
        public ICustomDataProcessor CreateProcessor(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processorEnvironment, nameof(processorEnvironment));
            Guard.NotNull(options, nameof(options));
            Guard.All(dataSources, x => x != null, nameof(dataSources));

            var processor = this.CreateProcessorCore(
                dataSources,
                processorEnvironment,
                options);

            if (processor is null)
            {
                throw new InvalidOperationException("CreateProcessorCore is not allowed to return null.");
            }

            return processor;
        }

        /// <inheritdoc />
        public ICustomDataProcessor CreateProcessor(
            IDataSourceGroup dataSourceGroup,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            Guard.NotNull(dataSourceGroup, nameof(dataSourceGroup));
            Guard.NotNull(processorEnvironment, nameof(processorEnvironment));
            Guard.NotNull(options, nameof(options));
            Guard.All(dataSourceGroup.DataSources, x => x != null, nameof(dataSourceGroup));

            var processor = this.CreateProcessorCore(
                dataSourceGroup,
                processorEnvironment,
                options);

            if (processor is null)
            {
                throw new InvalidOperationException("CreateProcessorCore is not allowed to return null.");
            }

            return processor;
        }

        /// <inheritdoc />
        public virtual Stream GetSerializationStream(SerializationSource source)
        {
            return null;
        }

        /// <inheritdoc />
        public bool IsDataSourceSupported(IDataSource dataSource)
        {
            if (dataSource is null)
            {
                return false;
            }

            bool isSupported;
            try
            {
                isSupported = this.IsDataSourceSupportedCore(dataSource);
            }
            catch
            {
                isSupported = false;
            }

            return isSupported;
        }

        /// <inheritdoc />
        public virtual void DisposeProcessor(ICustomDataProcessor processor)
        {
            // do nothing by default.
        }

        /// <summary>
        ///     When implemented in a derived class, creates a new
        ///     instance implementing <see cref="ICustomDataProcessor"/>
        ///     to process the specified data sources.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to be processed by the processor.
        ///     This parameter is guaranteed to be non-null, and all
        ///     elements in the collection are guaranteed to be non-null.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The options to pass to the processor.
        /// </param>
        /// <returns>
        ///     A new <see cref="ICustomDataProcessor"/>. It is an error
        ///     to return <c>null</c> from this method.
        /// </returns>
        protected abstract ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options);
        
        /// <summary>
        ///     When implemented in a derived class, creates a new
        ///     instance implementing <see cref="ICustomDataProcessor"/>
        ///     to process the specified <paramref name="dataSourceGroup"/>.
        /// </summary>
        /// <param name="dataSourceGroup">
        ///     The <see cref="IDataSourceGroup"/> to be processed by the processor.
        ///     This parameter is guaranteed to be non-null, and all
        ///     elements in the collection are guaranteed to be non-null.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The options to pass to the processor.
        /// </param>
        /// <returns>
        ///     A new <see cref="ICustomDataProcessor"/>. It is an error
        ///     to return <c>null</c> from this method.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     This <see cref="ProcessingSource"/> implements <see cref="IDataSourceGrouper"/>, but does not override
        ///     this method.
        /// </exception>
        protected virtual ICustomDataProcessor CreateProcessorCore(
            IDataSourceGroup dataSourceGroup,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            if (this is IDataSourceGrouper)
            {
                throw new InvalidOperationException(
                    $"Prior to V2, you must override the {nameof(CreateProcessorCore)} which accepts a {nameof(IDataSourceGroup)} when implementing {nameof(IDataSourceGrouper)}");
            }
            
            this.Logger.Warn($"{this.GetType().Name} does not support processing user-specified processing groups - falling back to default processing.");
	
            // Call v1 methods for now
            if (!(dataSourceGroup.ProcessingMode is DefaultProcessingMode))
            {
                this.Logger.Warn($"The {nameof(IProcessingMode)} of the {nameof(IDataSourceGroup)} passed to {nameof(CreateProcessorCore)} is not {nameof(DefaultProcessingMode)}, " +
                                 $"but {this.GetType().Name} does not implement {nameof(IDataSourceGrouper)}. This may indicate an error using the {nameof(IDataSourceGroup)} processing API.");
            }
            return this.CreateProcessor(dataSourceGroup.DataSources, processorEnvironment, options);
        }

        /// <summary>
        ///     When overridden in a derived class, returns a value indicating whether the
        ///     given Data Source can be opened by this instance.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source of interest.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance can actually process the given Data Source;
        ///     <c>false</c> otherwise.
        /// </returns>
        protected abstract bool IsDataSourceSupportedCore(IDataSource dataSource);

        /// <summary>
        ///     A derived class may implement this to perform additional actions when an <see cref="IApplicationEnvironment"/>
        ///     is made available.
        /// </summary>
        /// <param name="applicationEnvironment">
        ///     The handle back to the application environment.
        /// </param>
        protected virtual void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
        }

        /// <summary>
        ///     A derived class may implement this to perform additional actions when a logger is made available.
        /// </summary>
        /// <param name="logger">
        ///     The <seealso cref="ILogger"/> used to log information.
        /// </param>
        protected virtual void SetLoggerCore(ILogger logger)
        {
        }

        /// <summary>
        ///     A derived class may implement this to provide a custom <see cref="IProcessingSourceTableProvider"/>.
        /// </summary>
        /// <returns>
        ///     A table provider for the processing source.
        /// </returns>
        protected virtual IProcessingSourceTableProvider CreateTableProvider()
        {
            return this.tableProvider ?? TableDiscovery.CreateForAssembly(this.GetType().Assembly);
        }

        private void InitializeAllTables(ITableConfigurationsSerializer tableConfigSerializer)
        {
            Debug.Assert(tableConfigSerializer != null);

            IProcessingSourceTableProvider tableProvider = CreateTableProvider();
            if (tableProvider == null)
            {
                throw new InvalidOperationException($"{this.GetType().Name} returned a null table provider.");
            }

            var tables = tableProvider.Discover(tableConfigSerializer);

            var tableSet = new HashSet<TableDescriptor>();
            foreach (var table in tables)
            {
                if (!tableSet.Add(table))
                {
                    var error = string.Format(
                        CultureInfo.InvariantCulture,
                        "The table descriptor `{0}` appeared on more than one table in the ProcessingSource `{1}`. Tables must be unique according to their table descriptors.",
                        table,
                        this.GetType());
                    throw new InvalidOperationException(error);
                }

                if (table.IsMetadataTable)
                {
                    this.metadataTables.Add(table);
                }

                this.allTables.Add(table);
            }
        }
    }
}
