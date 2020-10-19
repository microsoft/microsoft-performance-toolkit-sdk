// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a base class for implementing custom data sources that
    ///     contains default logic for discovering tables, providing their
    ///     descriptors.
    /// </summary>
    public abstract class CustomDataSourceBase
        : ICustomDataSource
    {
        private readonly Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTables;
        private readonly ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesRO;

        private readonly HashSet<TableDescriptor> metadataTables;
        private readonly ReadOnlyHashSet<TableDescriptor> metadataTablesRO;

        private readonly Func<Assembly> tableAssemblyProvider;
        private readonly Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceBase"/>
        ///     class.
        /// </summary>
        protected CustomDataSourceBase()
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
            this.tableAssemblyProvider = () => this.GetType().Assembly;
            this.additionalTablesProvider = () => new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceBase"/>
        ///     class.
        /// </summary>
        /// <param name="additionalTablesProvider">
        ///     A function the provides an additional set of tables descriptors and their
        ///     underlying table types that should be exposed in addition to the tables
        ///     found via reflection.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="additionalTablesProvider"/> is <c>null</c>.
        /// </exception>
        protected CustomDataSourceBase(
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider)
        {
            Guard.NotNull(additionalTablesProvider, nameof(additionalTablesProvider));

            this.allTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            this.allTablesRO = new ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(this.allTables);
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);

            // See note in parameterless constructor
            this.tableAssemblyProvider = () => this.GetType().Assembly;
            this.additionalTablesProvider = additionalTablesProvider;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceBase"/>
        ///     class.
        /// </summary>
        /// <param name="additionalTablesProvider">
        ///     A function the provides an additional set of tables descriptors and their
        ///     underlying table types that should be exposed in addition to the tables
        ///     found via reflection.
        /// </param>
        /// <param name="tableAssemblyProvider">
        ///     A function that provides the <see cref="Assembly"/> that should be searched
        ///     for tables.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="additionalTablesProvider"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="tableAssemblyProvider"/> is <c>null</c>.
        /// </exception>
        protected CustomDataSourceBase(
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider,
            Func<Assembly> tableAssemblyProvider)
        {
            Guard.NotNull(additionalTablesProvider, nameof(additionalTablesProvider));
            Guard.NotNull(tableAssemblyProvider, nameof(tableAssemblyProvider));

            this.allTables = new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>();
            this.allTablesRO = new ReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(this.allTables);
            this.metadataTables = new HashSet<TableDescriptor>();
            this.metadataTablesRO = new ReadOnlyHashSet<TableDescriptor>(this.metadataTables);

            this.tableAssemblyProvider = tableAssemblyProvider;
            this.additionalTablesProvider = additionalTablesProvider;
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
        ///     This is set by default when the SetApplicationEnvironment is called by the runtime.
        ///     <seealso cref="IApplicationEnvironment"/>
        /// </summary>
        protected IApplicationEnvironment ApplicationEnvironment { get; private set; }

        /// <summary>
        ///     This is set when <see cref="SetLogger"/> is called by the runtime.
        ///     <seealso cref="ILogger"/>
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     When overridden in a derived class, saves the given environment into this instance.
        /// </summary>
        /// <param name="applicationEnvironment">
        ///     The handle back to the application environment.
        /// </param>
        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
            this.ApplicationEnvironment = applicationEnvironment;

            // Call derived class in case things need to be set before calling InitializeAllTables
            SetApplicationEnvironmentCore(applicationEnvironment);

            this.InitializeAllTables(this.tableAssemblyProvider, this.additionalTablesProvider, applicationEnvironment.Serializer);
        }

        /// <inheritdoc />
        public void SetLogger(ILogger logger)
        {
            this.Logger = logger;

            SetLoggerCore(logger);
        }

        /// <inheritdoc />
        public virtual CustomDataSourceInfo GetAboutInfo()
        {
            return new CustomDataSourceInfo
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
        public bool IsFileSupported(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                //
                // Empty file paths can never be supported, as they are
                // not valid paths in the first place.
                //

                return false;
            }

            bool isSupported;
            try
            {
                isSupported = this.IsFileSupportedCore(path);
            }
            catch
            {
                isSupported = false;
            }

            return isSupported;
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
        ///     When overridden in a derived class, determines whether the given
        ///     path to a specific source of data can in fact be processed by this
        ///     instance.
        /// </summary>
        /// <param name="path">
        ///     The path to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the path represents a source of data that can be processed
        ///     by this instance; <c>false</c> otherwise.
        /// </returns>
        protected abstract bool IsFileSupportedCore(string path);

        /// <summary>
        ///     When overridden in a derived class, gives environment into this instance.
        /// </summary>
        /// <param name="applicationEnvironment">
        ///     The handle back to the application environment.
        /// </param>
        protected abstract void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment);

        /// <summary>
        /// A derived class may implement this to perform additional actions when a logger is made available.
        /// </summary>
        /// <param name="logger">Use to log information. <seealso cref="ILogger"/></param>
        protected virtual void SetLoggerCore(ILogger logger)
        {
        }

        /// <summary>
        /// If a internal table does not provide a TableFactoryAttribute, this method may be implemented
        /// to generate an Action to create the table.
        /// </summary>
        /// <param name="type">Type associated with the table</param>
        /// <returns>An action to build the table, or null</returns>
        protected virtual Action<ITableBuilder, IDataExtensionRetrieval> GetTableBuildAction(
            Type type)
        {
            return null;
        }

        private void InitializeAllTables(
            Func<Assembly> tableAssemblyProvider,
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider,
            ISerializer tableConfigSerializer)
        {
            Debug.Assert(tableAssemblyProvider != null);
            Debug.Assert(additionalTablesProvider != null);
            Debug.Assert(tableConfigSerializer != null);

            var assembly = tableAssemblyProvider();
            if (assembly != null)
            {
                var allTableTypes = assembly.GetTypes();
                foreach (var tableType in allTableTypes)
                {
                    if (TableDescriptorFactory.TryCreate(
                        tableType,
                        tableConfigSerializer,
                        out bool internalTable,
                        out var descriptor,
                        out var buildTableAction) && internalTable)
                    {
                        if (descriptor.IsMetadataTable)
                        {
                            this.metadataTables.Add(descriptor);
                        }

                        this.allTables[descriptor] = buildTableAction ?? GetTableBuildAction(tableType);
                    }
                }
            }

            var additionalTables = additionalTablesProvider();
            if (additionalTables == null)
            {
                return;
            }

            foreach (var kvp in additionalTables)
            {
                this.allTables[kvp.Key] = kvp.Value;
                if (kvp.Key.IsMetadataTable)
                {
                    this.metadataTables.Add(kvp.Key);
                }
            }
        }
    }
}
