// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;

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
        private readonly Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTables;
        private readonly ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesRO;

        private readonly HashSet<TableDescriptor> metadataTables;
        private readonly ReadOnlyHashSet<TableDescriptor> metadataTablesRO;

        private readonly ITableProvider tableDiscoverer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSource"/>
        ///     class.
        /// </summary>
        protected ProcessingSource()
        {
            this.allTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            this.allTablesRO = new ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(this.allTables);
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);

            //
            // We need to have the full concrete type, and so must use this.GetType().
            // 'this' is not allowed to be used in a : this or : base call in the constructor,
            // so unfortunately we have to use an older idiom of every constructor calling an
            // init method so that we can pass the concrete type's assembly.
            //

            this.tableDiscoverer = TableDiscovery.CreateForAssembly(this.GetType().Assembly);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSource"/>
        ///     class.
        /// </summary>
        /// <param name="tableDiscoverer">
        ///     Object used to provide the tables that are exposed by this <see cref="ProcessingSource"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="tableDiscoverer"/> is <c>null</c>.
        /// </exception>
        protected ProcessingSource(ITableProvider tableDiscoverer)
        {
            Guard.NotNull(tableDiscoverer, nameof(tableDiscoverer));

            this.allTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            this.allTablesRO = new ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(this.allTables);
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);

            this.tableDiscoverer = tableDiscoverer;
        }

        /// <inheritdoc />
        public IEnumerable<TableDescriptor> DataTables => this.allTables.Keys
            .Except(this.metadataTablesRO);

        /// <inheritdoc />
        public IEnumerable<TableDescriptor> MetadataTables => this.metadataTablesRO;

        /// <inheritdoc />
        public virtual IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();

        /// <summary>
        ///     Gets a mapping of <see cref="TableDescriptor"/> to the concrete
        ///     <see cref="Type" /> of Table described by the descriptor. This
        ///     mapping includes the data and metadata tables.
        /// </summary>
        protected IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> AllTables => this.allTablesRO;

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
        ///     If a internal table does not provide a TableFactoryAttribute, this method may be implemented
        ///     to generate an Action to create the table.
        /// </summary>
        /// <param name="type">Type associated with the table</param>
        /// <returns>An action to build the table, or null</returns>
        protected virtual Action<ITableBuilder, IDataExtensionRetrieval> GetTableBuildAction(
            Type type)
        {
            return null;
        }

        private void InitializeAllTables(ISerializer tableConfigSerializer)
        {
            Debug.Assert(tableConfigSerializer != null);

            var tables = this.tableDiscoverer.Discover(tableConfigSerializer);

            var tableSet = new HashSet<DiscoveredTable>(DiscoveredTableEqualityComparer.ByTableDescriptor);
            foreach (var table in tables)
            {
                if (!tableSet.Add(table))
                {
                    var error = string.Format(
                        CultureInfo.InvariantCulture,
                        "The table descriptor `{0}` appeared on more than one table in the ProcessingSource `{1}`. Tables must be unique according to their table descriptors.",
                        table.Descriptor,
                        this.GetType());
                    throw new InvalidOperationException(error);
                }

                if (table.IsMetadata)
                {
                    this.metadataTables.Add(table.Descriptor);
                }

                this.allTables[table.Descriptor] = table.BuildTable ?? GetTableBuildAction(table.Descriptor.Type);
            }
        }
    }
}
