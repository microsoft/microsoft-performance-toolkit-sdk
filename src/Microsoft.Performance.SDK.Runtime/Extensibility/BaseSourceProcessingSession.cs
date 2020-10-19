// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     Provides the framework for processing a source, passing along parsed data elements to registered data cookers.
    /// </summary>
    /// <typeparam name="T">
    ///     Data element type.
    /// </typeparam>
    /// <typeparam name="TContext">
    ///     Data context type.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     Data element key type.
    /// </typeparam>
    internal abstract class BaseSourceProcessingSession<T, TContext, TKey>
        : ISourceProcessingSession<T, TContext, TKey>
          where T : IKeyedDataType<TKey>
    {
        private Dictionary<TKey, List<ISourceDataCooker<T, TContext, TKey>>> activeCookerRegistry;
        private List<ISourceDataCooker<T, TContext, TKey>> activeCookers;
        private HashSet<ISourceDataCooker<T, TContext, TKey>> activeReceiveAllDataCookers;

        private readonly Dictionary<DataCookerPath, ISourceDataCooker<T, TContext, TKey>> 
            cookerById = new Dictionary<DataCookerPath, ISourceDataCooker<T, TContext, TKey>>();

        private readonly HashSet<ISourceDataCooker<T, TContext, TKey>> 
            registeredCookers = new HashSet<ISourceDataCooker<T, TContext, TKey>>();

        private readonly HashSet<ISourceDataCooker<T, TContext, TKey>> 
            receiveAllDataCookers = new HashSet<ISourceDataCooker<T, TContext, TKey>>();

        private readonly IEqualityComparer<TKey> keyEqualityComparer;

        protected BaseSourceProcessingSession(
            ISourceParser<T, TContext, TKey> sourceParser, 
            IEqualityComparer<TKey> comparer)
        {
            Guard.NotNull(sourceParser, nameof(sourceParser));
            Guard.NotNull(comparer, nameof(comparer));

            this.SourceParser = sourceParser;
            this.keyEqualityComparer = comparer;
        }

        public ISourceParser<T, TContext, TKey> SourceParser { get; }

        public IReadOnlyCollection<ISourceDataCooker<T, TContext, TKey>> RegisteredSourceDataCookers 
            => this.RegisteredCookers;

        public string Id => this.SourceParser.Id;

        public Type DataElementType => this.SourceParser.DataElementType;

        public Type DataContextType => this.SourceParser.DataContextType;

        public Type DataKeyType => this.SourceParser.DataKeyType;

        public int MaxSourceParseCount => this.SourceParser.MaxSourceParseCount;

        protected IReadOnlyCollection<ISourceDataCooker<T, TContext, TKey>> RegisteredCookers => this.registeredCookers;

        public void RegisterSourceDataCooker(ISourceDataCooker<T, TContext, TKey> dataCooker)
        {
            Guard.NotNull(dataCooker, nameof(dataCooker));

            if (!StringComparer.Ordinal.Equals(dataCooker.Path.SourceParserId, SourceParser.Id))
            {
                throw new ArgumentException(
                    $"The {nameof(IDataCookerDescriptor.Path.SourceParserId)} of {nameof(dataCooker)} doesn't match "
                        + $"{nameof(SourceParser)}.", 
                    nameof(dataCooker));
            }

            if (!this.cookerById.ContainsKey(dataCooker.Path))
            {
                this.cookerById[dataCooker.Path] = dataCooker;
                this.registeredCookers.Add(dataCooker);

                if (dataCooker.Options.HasFlag(SourceDataCookerOptions.ReceiveAllDataElements))
                {
                    this.receiveAllDataCookers.Add(dataCooker);
                }
            }
        }

        public ISourceDataCooker<T, TContext, TKey> GetSourceDataCooker(DataCookerPath cookerPath)
        {
            Guard.NotNull(cookerPath, nameof(cookerPath ));

            this.cookerById.TryGetValue(cookerPath, out var cooker);
            return cooker;
        }

        public DataProcessingResult ProcessDataElement(T data, TContext context, CancellationToken cancellationToken)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            Debug.Assert(this.activeCookerRegistry != null);
            Debug.Assert(this.activeReceiveAllDataCookers != null);

            var key = data.GetKey();

            if (!this.activeCookerRegistry.ContainsKey(key) && !this.receiveAllDataCookers.Any())
            {
                return DataProcessingResult.Ignored;
            }

            DataProcessingResult ProcessDataCookers(IEnumerable<ISourceDataCooker<T, TContext, TKey>> cookers)
            {
                DataProcessingResult dataResult = DataProcessingResult.Ignored;

                foreach (var sourceDataCooker in cookers)
                {
                    var result = sourceDataCooker.CookDataElement(data, context, cancellationToken);
                    if (result == DataProcessingResult.CorruptData)
                    {
                        return result;
                    }

                    if (result == DataProcessingResult.Processed)
                    {
                        dataResult = result;
                    }
                }

                return dataResult;
            }

            var keyedResult = DataProcessingResult.Ignored;
            if (this.activeCookerRegistry.ContainsKey(key))
            {
                var sourceDataCookers = this.activeCookerRegistry[key];
                keyedResult = ProcessDataCookers(sourceDataCookers);
                if (keyedResult == DataProcessingResult.CorruptData)
                {
                    return keyedResult;
                }
            }

            var allDataResult = ProcessDataCookers(this.activeReceiveAllDataCookers);
            if (allDataResult == DataProcessingResult.CorruptData)
            {
                return allDataResult;
            }

            if (keyedResult == DataProcessingResult.Processed || allDataResult == DataProcessingResult.Processed)
            {
                return DataProcessingResult.Processed;
            }

            return DataProcessingResult.Ignored;
        }

        public abstract void ProcessSource(ILogger logger, IProgress<int> progress, CancellationToken cancellationToken);

        protected virtual void SetActiveDataCookers()
        {
            // this default implementation just adds all registered cookers

            ClearActiveCookers(this.registeredCookers.Count);
            foreach (var cooker in this.registeredCookers)
            {
                ActivateCooker(cooker);
            }
        }

        protected void InitializeForSourceParsing()
        {
            SetActiveDataCookers();
            this.RegisterCookers();
            this.InitializeSourceParserForProcessing();
        }

        protected void ClearActiveCookers(int newCapacityCount)
        {
            this.activeCookers = new List<ISourceDataCooker<T, TContext, TKey>>(newCapacityCount);
            this.activeReceiveAllDataCookers = new HashSet<ISourceDataCooker<T, TContext, TKey>>();
        }

        protected void ActivateCooker(ISourceDataCooker<T, TContext, TKey> sourceDataCooker)
        {
            Guard.NotNull(sourceDataCooker, nameof(sourceDataCooker));

            this.activeCookers.Add(sourceDataCooker);
            if (sourceDataCooker.Options.HasFlag(SourceDataCookerOptions.ReceiveAllDataElements))
            {
                this.activeReceiveAllDataCookers.Add(sourceDataCooker);
            }
        }

        private void RegisterCookers()
        {
            this.activeCookerRegistry = new Dictionary<TKey, List<ISourceDataCooker<T, TContext, TKey>>>(this.keyEqualityComparer);

            foreach (var sourceDataCooker in this.activeCookers)
            {
                if (sourceDataCooker.DataKeys == null)
                {
                    continue;
                }

                if (sourceDataCooker.Options.HasFlag(SourceDataCookerOptions.ReceiveAllDataElements))
                {
                    continue;
                }

                foreach (var key in sourceDataCooker.DataKeys)
                {
                    if (!this.activeCookerRegistry.ContainsKey(key))
                    {
                        this.activeCookerRegistry.Add(key, new List<ISourceDataCooker<T, TContext, TKey>>());
                    }

                    this.activeCookerRegistry[key].Add(sourceDataCooker);
                }
            }
        }

        private void InitializeSourceParserForProcessing()
        {
            this.SourceParser.PrepareForProcessing(this.activeReceiveAllDataCookers.Any(), activeCookerRegistry.Keys);
        }
    }
}
