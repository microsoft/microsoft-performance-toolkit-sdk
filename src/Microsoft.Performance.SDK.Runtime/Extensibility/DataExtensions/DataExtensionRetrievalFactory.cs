// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    ///// <summary>
    /////     This interface is used to query tables internal to an <see cref="ICustomDataProcessor"/>.
    ///// </summary>
    //internal interface IDataExtensionRetrievalInternal
    //{
    //    IDataExtensionRetrieval CreateDataRetrievalForTable(
    //        ITableExtensionReference tableExtensionReference);
    //}

    /// <summary>
    ///     All data extension objects, other than a source data cooker, require a set of data to pull
    ///     from. We need a way to restrict the data provided to the given data extension to just the data
    ///     coming from the data extensions it marked as required.
    ///
    ///     The purpose of this class is to generate that unique data set for a given data extension.
    ///
    ///     This restriction provides us the ability to quickly determine when a data extension is missing
    ///     a requirement, as well as to programatically enable a set of data extension from a list of tables,
    ///     so it is extremely important and full of general happiness.
    /// </summary>
    public class DataExtensionRetrievalFactory
        : IDataExtensionRetrievalFactory
    {
        private readonly DataRetrievalCache dataRetrievalCache = new DataRetrievalCache();

        private readonly IProcessingSystemCookerData cookerData;
        //private readonly IProcessingSystemCookerData cookerData;
        private IDataExtensionRepository DataExtensionRepository { get; }

        //public DataExtensionRetrievalFactory(
        //    ICookedDataRetrieval sourceCookerData,
        //    ICompositeCookerRetrieval compositeCookers,
        //    IDataExtensionRepository dataExtensionRepository)
        //    : this(new ProcessingSystemCookerData(sourceCookerData, compositeCookers), dataExtensionRepository)
        //{
        //}

        /// <summary>
        ///     Initialize an instance of this class.
        /// </summary>
        /// <param name="cookerData">
        ///     Provides access to cookers and cooker data.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides a way to generate data extensions other than source data cookers.
        /// </param>
        public DataExtensionRetrievalFactory(
            IProcessingSystemCookerData cookerData,
            IDataExtensionRepository dataExtensionRepository)
        {
            Guard.NotNull(cookerData, nameof(cookerData));
            Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));

            this.cookerData = cookerData;
            this.DataExtensionRepository = dataExtensionRepository;
        }

        /// <summary>
        ///     A composite cooker has access to source data cookers, as well as other composite data cookers.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     Identifies the composite data cooker.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this composite data cooker.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForCompositeDataCooker(
            DataCookerPath dataCookerPath)
        {
            var filteredData = this.dataRetrievalCache.GetCompositeDataCookerFilteredData(dataCookerPath);
            if (filteredData != null)
            {
                return filteredData;
            }

            var compositeDataCookerReference = this.DataExtensionRepository.GetCompositeDataCookerReference(dataCookerPath);

            if (compositeDataCookerReference == null)
            {
                throw new ArgumentException("Data retrieval requested for data cooker not found in repository.");
            }

            if (compositeDataCookerReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for data cooker that is not available.");
            }

            filteredData = new FilteredDataRetrieval(
                this.cookerData,
                this.DataExtensionRepository,
                this.CreateDataRetrievalForCompositeDataCooker,
                this.CreateDataRetrievalForDataProcessor,
                compositeDataCookerReference.DependencyReferences);

            this.dataRetrievalCache.AddCompositeDataCookerFilteredData(dataCookerPath, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A data processor has access to source data cookers, composite data cookers, as well as other
        ///     data processors.
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Identifies the data processor.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this data processor.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(
            DataProcessorId dataProcessorId)
        {
            var filteredData = this.dataRetrievalCache.GetDataProcessorFilteredData(dataProcessorId);
            if (filteredData != null)
            {
                return filteredData;
            }

            var dataProcessorReference = this.DataExtensionRepository.GetDataProcessorReference(dataProcessorId);

            if (dataProcessorReference == null)
            {
                throw new ArgumentException("Data retrieval requested for data processor not found in repository.");
            }

            if (dataProcessorReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for data processor that is not available.");
            }

            filteredData = new FilteredDataRetrieval(
                this.cookerData,
                this.DataExtensionRepository,
                this.CreateDataRetrievalForCompositeDataCooker,
                this.CreateDataRetrievalForDataProcessor,
                dataProcessorReference.DependencyReferences);

            this.dataRetrievalCache.AddDataProcessorFilteredData(dataProcessorId, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A table has access to source data cookers, composite data cookers, and data processors.
        /// </summary>
        /// <param name="tableId">
        ///     Identifies the table.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this table.
        /// </returns>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(
            Guid tableId)
        {
            if (tableId == Guid.Empty)
            {
                throw new ArgumentException($"The table Id may not be {nameof(Guid.Empty)}.", nameof(tableId));
            }

            var filteredData = this.dataRetrievalCache.GetTableFilteredData(tableId);
            if (filteredData != null)
            {
                return filteredData;
            }

            if (!this.DataExtensionRepository.TablesById.TryGetValue(tableId, out var tableExtensionReference))
            {
                throw new ArgumentException(
                    $"The table Id reference was not found in the data extension repository.",
                    nameof(tableId));
            }

            Debug.Assert(tableExtensionReference != null);

            filteredData = this.CreateDataRetrievalForTable(tableExtensionReference);

            this.dataRetrievalCache.AddTableFilteredData(tableId, filteredData);

            return filteredData;
        }

        /// <summary>
        ///     A table has access to source data cookers, composite data cookers, and data processors.
        /// </summary>
        /// <param name="tableExtensionReference">
        ///     Reference to a table data extension.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this table.
        /// </returns>
        internal IDataExtensionRetrieval CreateDataRetrievalForTable(
            ITableExtensionReference tableExtensionReference)
        {
            Guard.NotNull(tableExtensionReference, nameof(tableExtensionReference));

            if (tableExtensionReference.Availability != DataExtensionAvailability.Available)
            {
                throw new ArgumentException("Data retrieval requested for table that is not available.");
            }

            var filteredData = new FilteredDataRetrieval(
                this.cookerData,
                this.DataExtensionRepository,
                this.CreateDataRetrievalForCompositeDataCooker,
                this.CreateDataRetrievalForDataProcessor,
                tableExtensionReference.DependencyReferences);

            return filteredData;
        }

        //internal IDataCooker GetOrCreateCompositeDataCooker(DataCookerPath cookerPath)
        //{
        //    return this.cookerData.GetOrCreateCompositeCooker(cookerPath);
        //}
    }

    /// <summary>
    ///     This owns all instances of composite cookers for a given system.
    /// </summary>
    public sealed class ProcessingSystemCompositeCookers
          : ICompositeCookerRepository,
            IDisposable
    {
        private readonly ReaderWriterLockSlim compositeCookerLock = new ReaderWriterLockSlim();

        private readonly ConcurrentDictionary<DataCookerPath, IDataCooker> compositeCookersByPath
            = new ConcurrentDictionary<DataCookerPath, IDataCooker>();

        private readonly IDataExtensionRepository dataExtensionRepository;
        private bool disposedValue;

        public ProcessingSystemCompositeCookers(
            IDataExtensionRepository dataExtensionRepository)
        {
            this.dataExtensionRepository = dataExtensionRepository;
        }

        public ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            this.ThrowIfDisposed();

            if (this.compositeCookersByPath.TryGetValue(cookerPath, out var dataCooker))
            {
                return dataCooker;
            }

            var compositeCookerReference = this.dataExtensionRepository.GetCompositeDataCookerReference(cookerPath);
            if (compositeCookerReference == null)
            {
                throw new InvalidOperationException(
                    $"The data extension repository is missing expected composite data cooker: {cookerPath}");
            }

            if (createDataRetrieval == null)
            {
                throw new ArgumentNullException(
                    message: "The function is null, unable to create the composite data cooker.",
                    paramName: nameof(createDataRetrieval));
            }

            var compositeCooker = compositeCookerReference.CreateInstance(createDataRetrieval(cookerPath));
            if (compositeCooker == null)
            {
                throw new InvalidOperationException(
                    $"The composite cooker reference returned null cooker: {cookerPath}. Was it properly initialized?");
            }

            return this.compositeCookersByPath.GetOrAdd(cookerPath, compositeCooker);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var cooker in this.compositeCookersByPath.Values)
                    {
                        cooker.TryDispose();
                    }

                    this.compositeCookersByPath.Clear();

                    this.compositeCookerLock.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }

    ///// <summary>
    /////     This filters a set of composite cookers down based on a set of available source parsers. This class does not
    /////     own composite cooker instances.
    ///// </summary>
    //internal sealed class FilteredCompositeCookers
    //    : ICompositeCookerRetrieval
    //{
    //    private readonly ICompositeCookerRetrieval allCookers;
    //    private readonly IEnumerable<string> availableSourceParsers;

    //    internal FilteredCompositeCookers(
    //        ICompositeCookerRetrieval allCookers,
    //        IEnumerable<string> availableSourceParsers)
    //    {
    //        Debug.Assert(allCookers != null);
    //        Debug.Assert(availableSourceParsers != null);

    //        this.allCookers = allCookers;
    //        this.availableSourceParsers = availableSourceParsers;
    //    }

    //    public void Dispose()
    //    {
    //        // nothing to do here, it doesn't own the data cooker instances
    //    }

    //    public IDataCooker GetOrCreateCompositeCooker(DataCookerPath cookerPath)
    //    {
    //        // todo: implement filtering
    //        return allCookers.GetOrCreateCompositeCooker(cookerPath);
    //    }
    //}

    /// <summary>
    ///     This unifies the cooker data access for a given processing system.
    /// </summary>
    /// <remarks>
    ///     This must be implemented to return data for both Source Data Cookers as well as Composite Data Cookers.
    /// </remarks>
    public interface IProcessingSystemCookerData
        : ICompositeCookerRepository
    {
        ICookedDataRetrieval SourceCookerData { get; }
    }

    public class ProcessingSystemCookerData
        : IProcessingSystemCookerData
    {
        private readonly ICookedDataRetrieval sourceCookerData;
        private readonly ICompositeCookerRepository compositeCookers;

        private bool disposedValue;

        //public ProcessingSystemCookerData(
        //    ICookedDataRetrieval sourceCookerData,
        //    ProcessingSystemCompositeCookers compositeCookers,
        //    IDataExtensionRepository extensionRepository)
        //{
        //    Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
        //    Guard.NotNull(extensionRepository, nameof(extensionRepository));

        //    this.sourceCookerData = sourceCookerData;
        //    this.compositeCookers = compositeCookers;
        //}

        public ProcessingSystemCookerData(
            ICookedDataRetrieval sourceCookerData,
            ICompositeCookerRepository compositeCookerData)
        {
            Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
            Guard.NotNull(compositeCookerData, nameof(compositeCookerData));

            this.sourceCookerData = sourceCookerData;
            this.compositeCookers = compositeCookerData;
        }

        public ProcessingSystemCookerData(
            ICookedDataRetrieval sourceCookerData,
            ProcessingSystemCookerData other)
        {
            Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
            Guard.NotNull(other, nameof(other));

            this.sourceCookerData = sourceCookerData;
            this.compositeCookers = other.compositeCookers;
        }

        public ICookedDataRetrieval SourceCookerData => this.sourceCookerData;

        public ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval)
        {
            return this.compositeCookers.GetOrCreateCompositeCooker(cookerPath, createDataRetrieval);
        }

        //public T QueryOutput<T>(DataOutputPath identifier)
        //{
        //    if (identifier.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
        //    {
        //        // this is a source cooker, so it already exists
        //        return this.sourceCookerData.QueryOutput<T>(identifier);
        //    }
        //    else
        //    {
        //        // todo: should we handle this here? I like that it hides the differences between cooker types, so
        //        // taking that approach for now.

        //        // I'm going to remove the IProcessingSystemCookerData interface for now, and just use ICookedDataRetrieval. See if that works...

        //        //// composite cooker data should be accessed by calling GetOrCreateCompositeCooker
        //        //throw new InvalidOperationException("This should not be queried for composite cookers.");

        //        var compositeCooker = GetOrCreateCompositeCooker(identifier.CookerPath);
        //        if (compositeCooker == null)
        //        {
        //            // todo: is there a better exception for this?
        //            throw new ArgumentException(
        //                message: "The identified composite cooker was not available.",
        //                paramName: nameof(identifier));
        //        }

        //        return compositeCooker.QueryOutput<T>(identifier);
        //    }
        //}

        //public object QueryOutput(DataOutputPath identifier)
        //{
        //    if (identifier.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
        //    {
        //        // this is a source cooker, so it already exists
        //        return this.sourceCookerData.QueryOutput(identifier);
        //    }
        //    else
        //    {
        //        // todo: should we handle this here? I like that it hides the differences between cooker types, so
        //        // taking that approach for now.

        //        // I'm going to remove the IProcessingSystemCookerData interface for now, and just use ICookedDataRetrieval. See if that works...

        //        //// composite cooker data should be accessed by calling GetOrCreateCompositeCooker
        //        //throw new InvalidOperationException("This should not be queried for composite cookers.");

        //        var compositeCooker = GetOrCreateCompositeCooker(identifier.CookerPath);
        //        if (compositeCooker == null)
        //        {
        //            // todo: is there a better exception for this?
        //            throw new ArgumentException(
        //                message: "The identified composite cooker was not available.",
        //                paramName: nameof(identifier));
        //        }

        //        return compositeCooker.QueryOutput(identifier);
        //    }
        //}

        //public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        //{
        //    try
        //    {
        //        result = QueryOutput<T>(identifier);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        result = default;
        //        return false;
        //    }
        //}

        //public bool TryQueryOutput(DataOutputPath identifier, out object result)
        //{
        //    try
        //    {
        //        result = QueryOutput(identifier);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        result = default;
        //        return false;
        //    }
        //}

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // this doesn't own any disposable objects, so nothing to do
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    ///     For tables that are built through an <see cref="ICustomDataProcessor"/>, this is used to map a table to its
    ///     data processor.
    /// </summary>
    public interface IMapTablesToProcessors
    {
        /// <summary>
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </summary>
        IReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> TableToProcessor { get; }
    }

    /// <summary>
    ///     Provides access to extensions data associated with either a single or a grouped set of 
    ///     <see cref="ICustomDataProcessor"/>s.
    /// </summary>
    public interface IProcessingSessionData
        : ICookedDataRetrieval,
          ICompositeCookerRetrieval,
          IMapTablesToProcessors,
          IDataExtensionRetrievalFactory,
          IDisposable
    {
        // todo: Replace ICookedDataRetrieval with ISourceCookerRetrieval instead for consistency.
    }

    /// <summary>
    ///     Provides access to extensions data associated with either a single or a grouped set of 
    ///     <see cref="ICustomDataProcessor"/>s.
    /// </summary>
    public sealed class ProcessingSystemData
        : IProcessingSessionData,
          IDisposable
    {
        private readonly ProcessingSystemCookerData cookerDataRetrieval;
        private DataExtensionRetrievalFactory dataExtensionRetrievalFactory;
        //private ProcessingSystemCompositeCookers compositeCookerDataRetrieval;
        private ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessor;

        private bool disposedValue;

        /// <summary>
        ///     Initializes an instance of <see cref="ProcessingSystemData"/>.
        /// </summary>
        /// <param name="cookerDataRetrieval">
        ///     Provides access to all available cooker data.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to data extensions.
        /// </param>
        /// <param name="tableToProcessorMap">
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="cookerDataRetrieval"/> is null.
        ///     - or -
        ///     <paramref name="dataExtensionRepository"/> is null.
        ///     - or -
        ///     <paramref name="tableToProcessorMap"/> is null.
        /// </exception>
        public ProcessingSystemData(
            ProcessingSystemCookerData cookerDataRetrieval,
            IDataExtensionRepository dataExtensionRepository,
            IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap)
        {
            Guard.NotNull(cookerDataRetrieval, nameof(cookerDataRetrieval));
            Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));
            Guard.NotNull(tableToProcessorMap, nameof(tableToProcessorMap));

            this.cookerDataRetrieval = cookerDataRetrieval;
            this.dataExtensionRetrievalFactory = new DataExtensionRetrievalFactory(this.cookerDataRetrieval, dataExtensionRepository);
            //this.compositeCookerDataRetrieval = new ProcessingSystemCompositeCookers(dataExtensionRepository);
            this.tableToProcessor = new ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor>(tableToProcessorMap);
        }

        /// <summary>
        ///     Initializes an instance of <see cref="ProcessingSystemData"/>.
        /// </summary>
        /// <param name="sourceDataRetrieval">
        ///     Provides access to source cooker data.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to data extensions.
        /// </param>
        /// <param name="tableToProcessorMap">
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourceDataRetrieval"/> is null.
        ///     - or -
        ///     <paramref name="dataExtensionRepository"/> is null.
        ///     - or -
        ///     <paramref name="tableToProcessorMap"/> is null.
        /// </exception>
        //public ProcessingSessionData(
        //    ICookedDataRetrieval sourceDataRetrieval,
        //    DataExtensionRetrievalFactory dataExtensionRetrievalFactory,
        //    IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap)
        //{
        //    Guard.NotNull(sourceDataRetrieval, nameof(sourceDataRetrieval));
        //    Guard.NotNull(dataExtensionRepository, nameof(dataExtensionRepository));
        //    Guard.NotNull(tableToProcessorMap, nameof(tableToProcessorMap));

        //    this.cookerDataRetrieval = sourceDataRetrieval;
        //    this.dataExtensionRetrievalFactory = dataExtensionRetrievalFactory;
        //    //this.compositeCookerDataRetrieval = new ProcessingSessionCompositeCookers(
        //    //    this.DataExtensionRetrievalFactory,
        //    //    dataExtensionRepository);
        //    this.tableToProcessor = new ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor>(tableToProcessorMap);
        //}

        ///// <summary>
        /////     Initializes an instance of <see cref="ProcessingSessionData"/>.
        ///// </summary>
        ///// <param name="sourceDataRetrieval">
        /////     Provides access to source cooker data.
        ///// </param>
        ///// <param name="dataExtensionRepository">
        /////     Provides access to data extensions.
        ///// </param>
        ///// <param name="tableToProcessorMap">
        /////     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        ///// </param>
        ///// <exception cref="ArgumentNullException">
        /////     <paramref name="sourceDataRetrieval"/> is null.
        /////     - or -
        /////     <paramref name="dataExtensionRepository"/> is null.
        /////     - or -
        /////     <paramref name="tableToProcessorMap"/> is null.
        ///// </exception>
        //internal ProcessingSessionData(
        //    ProcessingSessionData other,
        //    ICookedDataRetrieval sourceDataRetrieval,
        //    IEnumerable<string> availableSourceParserIds)
        //{
        //    Guard.NotNull(other, nameof(other));
        //    Guard.NotNull(availableSourceParserIds, nameof(availableSourceParserIds));

        //    this.sourceCookerDataRetrieval = sourceDataRetrieval;
        //    this.dataExtensionRetrievalFactory = new DataExtensionRetrievalFactory(this, dataExtensionRepository);
        //    this.compositeCookerDataRetrieval = new ProcessingSessionCompositeCookers(
        //        this.DataExtensionRetrievalFactory,
        //        dataExtensionRepository);
        //    this.tableToProcessor = new ReadOnlyDictionary<TableDescriptor, ICustomDataProcessor>(tableToProcessorMap);
        //}

        /// <summary>
        ///     Provides access to source data cooker data from one or more data processors associated with a 
        ///     procesesing session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private ICookedDataRetrieval SourceCookerDataRetrieval
        {
            get
            {
                this.ThrowIfDisposed();
                return this.cookerDataRetrieval.SourceCookerData;
            }
        }

        ///// <summary>
        /////     Provides access to composite data cookers for the processing session.
        ///// </summary>
        ///// <exception cref="ObjectDisposedException">
        /////     This instance is disposed.
        ///// </exception>
        //private ProcessingSystemCompositeCookers CompositeCookerDataRetrieval
        //{
        //    get
        //    {
        //        this.ThrowIfDisposed();
        //        return this.compositeCookerDataRetrieval;
        //    }
        //}

        /// <summary>
        ///     Provides access to a <see cref="IDataExtensionRetrievalFactory"/> for the processing session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private DataExtensionRetrievalFactory DataExtensionRetrievalFactory
        {
            get
            {
                this.ThrowIfDisposed();
                return this.dataExtensionRetrievalFactory;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> TableToProcessor
        {
            get
            {
                this.ThrowIfDisposed();
                return this.tableToProcessor;
            }
        }

        internal ProcessingSystemCookerData CookerData
        {
            get
            {
                this.ThrowIfDisposed();
                return this.CookerData;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public T QueryOutput<T>(DataOutputPath identifier)
        {
            return this.SourceCookerDataRetrieval.QueryOutput<T>(identifier);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public object QueryOutput(DataOutputPath identifier)
        {
            return this.SourceCookerDataRetrieval.QueryOutput(identifier);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        {
            return this.SourceCookerDataRetrieval.TryQueryOutput(identifier, out result);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput(DataOutputPath identifier, out object result)
        {
            return this.SourceCookerDataRetrieval.TryQueryOutput(identifier, out result);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public ICookedDataRetrieval GetCompositeCookerDataRetrieval(DataCookerPath cookerPath)
        {
            return this.cookerDataRetrieval.GetOrCreateCompositeCooker(
                cookerPath,
                this.dataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForCompositeDataCooker(
            DataCookerPath dataCookerPath)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(dataCookerPath);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(DataProcessorId dataProcessorId)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForDataProcessor(dataProcessorId);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(Guid tableId)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableId);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionRetrieval CreateDataRetrievalForTable(ITableExtensionReference tableExtensionReference)
        {
            return this.DataExtensionRetrievalFactory.CreateDataRetrievalForTable(tableExtensionReference);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //this.compositeCookerDataRetrieval.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
    }
}
