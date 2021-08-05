using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.Toolkit.Engine
{
    public sealed class DataSourceSet
        : IDisposable
    {
        private readonly EngineCreateInfo createInfo;
        private readonly ExtensionRoot extensionRoot;
        private readonly ReadOnlyCollection<string> extensionDirectories;
        private readonly DataExtensionFactory factory;

        private readonly Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess;
        private readonly List<ProcessingSourceReference> processingSourceReferences;
        private readonly List<IDataSource> freeDataSources;

        private readonly IAssemblyLoader assemblyLoader;

        private IEnumerable<ErrorInfo> creationErrors;

        private bool isDisposed;

        public DataSourceSet()
            : this(new EngineCreateInfo())
        {
        }

        public DataSourceSet(EngineCreateInfo createInfo)
        {
            Guard.NotNull(createInfo, nameof(createInfo));

            this.createInfo = createInfo;
            this.freeDataSources = new List<IDataSource>();
            this.dataSourcesToProcess = new Dictionary<ProcessingSourceReference, List<List<IDataSource>>>();

            IPlugInCatalog catalog = null;
            IDataExtensionRepositoryBuilder repo = null;

            try
            {
                this.extensionDirectories = createInfo.ExtensionDirectories.ToList().AsReadOnly();
                Debug.Assert(this.extensionDirectories.Any());

                this.assemblyLoader = createInfo.AssemblyLoader ?? new AssemblyLoader();

                var assemblyDiscovery = new AssemblyExtensionDiscovery(this.assemblyLoader, _ => new NullValidator());

                catalog = new ReflectionPlugInCatalog(assemblyDiscovery);

                this.factory = new DataExtensionFactory();
                repo = this.factory.CreateDataExtensionRepository();

                var reflector = new DataExtensionReflector(
                    assemblyDiscovery,
                    repo);

                assemblyDiscovery.ProcessAssemblies(this.extensionDirectories, out var discoveryError);

                repo.FinalizeDataExtensions();

                this.CreationErrors = new[] { discoveryError, };

                this.extensionRoot = new ExtensionRoot(catalog, repo);
                catalog = null;
                repo = null;

                this.processingSourceReferences = new List<ProcessingSourceReference>(catalog.PlugIns);
            }
            catch (Exception)
            {
                repo.SafeDispose();
                catalog.SafeDispose();
                throw;
            }
        }

        internal EngineCreateInfo CreateInfo
        {
            get
            {
                this.ThrowIfDisposed();
                return this.createInfo;
            }
        }

        internal ExtensionRoot Extensions
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionRoot;
            }
        }

        internal DataExtensionFactory Factory
        {
            get
            {
                this.ThrowIfDisposed();
                return this.factory;
            }
        }

        internal ReadOnlyCollection<string> ExtensionDirectories
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDirectories;
            }
        }

        internal List<ProcessingSourceReference> ProcessingSourceReferences
        {
            get
            {
                this.ThrowIfDisposed();
                return this.processingSourceReferences;
            }
        }

        internal IAssemblyLoader AssemblyLoader
        {
            get
            {
                this.ThrowIfDisposed();
                return this.assemblyLoader;
            }
        }

        // do we want this? i'm thinking no - it makes it harder for us to add other Engine implementations,
        // but I can see how it might be nice to the customer.
        //public Engine CreateEngine()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        ///     Gets any non-fatal errors that occured during engine
        ///     creation. Because these errors are not fatal, they are
        ///     reported here rather than raising an exception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<ErrorInfo> CreationErrors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.creationErrors;
            }
            private set
            {
                Debug.Assert(!this.isDisposed);
                this.ThrowIfDisposed();
                this.creationErrors = value;
            }
        }

        /// <summary>
        ///     Adds the given file to this instance for processing.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="dataSource"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public void AddDataSource(IDataSource dataSource)
        {
            this.ThrowIfDisposed();
            Guard.NotNull(dataSource, nameof(dataSource));

            if (!this.TryAddDataSource(dataSource))
            {
                throw new UnsupportedDataSourceException(dataSource);
            }
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing.
        /// </summary>
        /// <param name="filePath">
        ///     The Data Source to process.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source has been added for processing;
        ///     <c>false</c> if the Data Source is not valid, cannot be processed,
        ///     or the instance has already been processed. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource)
        {
            this.ThrowIfDisposed();
            if (dataSource is null)
            {
                return false;
            }

            if (!this.processingSourceReferences.Any(x => x.Supports(dataSource)))
            {
                return false;
            }

            try
            {
                this.freeDataSources.Add(dataSource);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Adds the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public void AddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            this.AddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process <paramref name="dataSource"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/> has been added for processing by the <see cref="IProcessingSource"/>;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            return this.TryAddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Adds the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public void AddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            this.ThrowIfDisposed();
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processingSourceType, nameof(processingSourceType));
            if (dataSources.Any(x => x is null))
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferences, this.dataSourcesToProcess, this.TypeIs);
        }

        /// <summary>
        ///     Attempts to add the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            this.ThrowIfDisposed();

            if (dataSources is null ||
                dataSources.Any(x => x is null) ||
                processingSourceType is null)
            {
                return false;
            }

            try
            {
                this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferences, this.dataSourcesToProcess, this.TypeIs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TypeIs(Type first, Type second)
        {
            Debug.Assert(!this.isDisposed);
            Debug.Assert(first != null);
            Debug.Assert(second != null);

            if (this.assemblyLoader.SupportsIsolation)
            {
                return first.GUID == second.GUID &&
                       first.AssemblyQualifiedName == second.AssemblyQualifiedName;
            }
            else
            {
                return first.Is(second);
            }
        }

        private void AddDataSourcesCore(
            IEnumerable<IDataSource> dataSources,
            Type processingSourceType,
            List<ProcessingSourceReference> processingSourceReferences,
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Func<Type, Type, bool> typeIs)
        {
            Debug.Assert(!this.isDisposed);
            Debug.Assert(dataSources != null);
            Debug.Assert(processingSourceType != null);
            Debug.Assert(processingSourceReferences != null);
            Debug.Assert(dataSourcesToProcess != null);
            Debug.Assert(typeIs != null);

            var cdsr = processingSourceReferences.FirstOrDefault(x => typeIs(x.Instance.GetType(), processingSourceType));
            if (cdsr is null)
            {
                throw new UnsupportedProcessingSourceException(processingSourceType);
            }

            var atLeastOneDataSourceProvided = false;
            foreach (var dataSource in dataSources)
            {
                Debug.Assert(dataSource != null);

                atLeastOneDataSourceProvided = true;
                if (!cdsr.Supports(dataSource))
                {
                    throw new UnsupportedDataSourceException(dataSource, processingSourceType);
                }
            }

            if (!atLeastOneDataSourceProvided)
            {
                throw new ArgumentException("The Data Source collection cannot be empty.", nameof(dataSources));
            }

            if (!dataSourcesToProcess.TryGetValue(cdsr, out var list))
            {
                list = new List<List<IDataSource>>();
                dataSourcesToProcess[cdsr] = list;
            }

            list.Add(dataSources.ToList());
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Engine));
            }
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private sealed class NullValidator
            : IPreloadValidator
        {
            public bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error)
            {
                error = ErrorInfo.None;
                return true;
            }

            public void Dispose()
            {
            }
        }
    }
}
