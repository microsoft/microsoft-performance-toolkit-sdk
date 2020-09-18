// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     This abstract class provides default implementations for source data cookers.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="Type"/> of the data element from the source parser.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     <see cref="Type"/> of contextual information for the data element.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Identifier for the <see cref="Type"/> of data element.
    /// </typeparam>
    public abstract class BaseSourceDataCooker<T, TContext, TKey>
        : CookedDataReflector,
          ISourceDataCooker<T, TContext, TKey>
        where T : IKeyedDataType<TKey>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseSourceDataCooker{T, TContext, TKey}"/>
        ///     class for the given cooker.
        /// </summary>
        /// <param name="sourceId">
        ///     The ID of the source parser from which this cooker cooks
        ///     data.
        /// </param>
        /// <param name="cookerId">
        ///     This cooker's ID.
        /// </param>
        protected BaseSourceDataCooker(string sourceId, string cookerId)
            : this(new DataCookerPath(sourceId, cookerId))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseSourceDataCooker{T, TContext, TKey}"/>
        ///     class for the given cooker.
        /// </summary>
        /// </param>
        /// <param name="dataCookerPath">
        ///     This cooker's path.
        /// </param>
        protected BaseSourceDataCooker(DataCookerPath dataCookerPath)
            : base(dataCookerPath)
        {
            this.Path = dataCookerPath;
        }

        /// <summary>
        ///     Gets the description of this cooker.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets a collection of the keys for all data
        ///     produced by this cooker.
        /// </summary>
        public abstract ReadOnlyHashSet<TKey> DataKeys { get; }

        /// <summary>
        ///     Gets the path of this cooker.
        /// </summary>
        public virtual DataCookerPath Path { get; }

        /// <summary>
        ///     When overridden in a derived class, processes
        ///     a data element of the source cooker.
        /// </summary>
        /// <param name="data">
        ///     The data item being cooked.
        /// </param>
        /// <param name="context">
        ///     The overall context of the cooking session.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The outcome of processing the data item.
        /// </returns>
        public abstract DataProcessingResult CookDataElement(
            T data,
            TContext context,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets the options for this instance.
        ///     <para />
        ///     By default, there are no options. Override this
        ///     member in order to provide different options to
        ///     the cooker.
        /// </summary>
        public virtual SourceDataCookerOptions Options => SourceDataCookerOptions.None;

        /// <summary>
        ///     Gets all dependencies of this cooker.
        ///     <para />
        ///     By default, there are no dependencies. Override this
        ///     member in order to declare dependencies for this
        ///     cooker.
        /// </summary>
        public virtual IReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> DependencyTypes { get; }
            = new Dictionary<DataCookerPath, DataCookerDependencyType>();

        /// <summary>
        ///     Gets the data production strategy used by this cooker.
        ///     <para />
        ///     By default, the strategy is <see cref="DataProductionStrategy.PostSourceParsing"/>.
        ///     Override this member in order to use a different <see cref="DataProductionStrategy"/>.
        /// </summary>
        public virtual DataProductionStrategy DataProductionStrategy => DataProductionStrategy.PostSourceParsing;

        /// <summary>
        ///     Gets the cookers required by this cooker in order to successfully
        ///     process data.
        ///     <para/>
        ///     By default, there are no required cookers. Override this member
        ///     in order to declare additional requirements.
        /// </summary>
        public virtual IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; } 
            = new ReadOnlyCollection<DataCookerPath>(new List<DataCookerPath>());

        /// <summary>
        ///     When overridden in a derived class, performs any processing that
        ///     must be performed before cooking actually begins. By default,
        ///     this method does nothing.
        /// </summary>
        /// <param name="dependencyRetrieval">
        ///     Provides a means of retrieving the dependencies of this cooker.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        public virtual void BeginDataCooking(ICookedDataRetrieval dependencyRetrieval, CancellationToken cancellationToken)
        {
        }

        /// <summary>
        ///     When overridden in a derived class, performs any processing
        ///     that should occur after source parsing completes. By default,
        ///     this method does nothing.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        public virtual void EndDataCooking(CancellationToken cancellationToken)
        {
        }
    }
}
