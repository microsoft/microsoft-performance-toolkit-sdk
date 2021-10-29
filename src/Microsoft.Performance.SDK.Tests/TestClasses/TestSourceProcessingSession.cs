// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestSourceProcessingSession<T, TContext, TKey>
        : ISourceProcessingSession<T, TContext, TKey>
          where T : IKeyedDataType<TKey>
    {
        public ICustomDataProcessorWithSourceParser<T, TContext, TKey> CustomDataProcessor { get; set; }

        public string Id { get; set; }

        public Type DataElementType { get; set; } = typeof(T);

        public Type DataContextType { get; set; } = typeof(TContext);

        public Type DataKeyType { get; set; } = typeof(TKey);

        public int MaxSourceParseCount { get; set; } = int.MaxValue;

        public List<ISourceDataCooker<T, TContext, TKey>> SourceDataCookers = new List<ISourceDataCooker<T, TContext, TKey>>();

        public IReadOnlyCollection<ISourceDataCooker<T, TContext, TKey>> RegisteredSourceDataCookers =>
            SourceDataCookers;

        public Func<DataCookerPath, ISourceDataCooker<T, TContext, TKey>> GetSourceDataCookerFunc { get; set; }
        public ISourceDataCooker<T, TContext, TKey> GetSourceDataCooker(DataCookerPath cookerPath)
        {
            if (GetSourceDataCookerFunc != null)
            {
                return GetSourceDataCookerFunc.Invoke(cookerPath);
            }

            foreach (var cooker in SourceDataCookers)
            {
                if (cooker.Path == cookerPath)
                {
                    return cooker;
                }
            }

            return null;
        }

        public Func<T, TContext, CancellationToken, DataProcessingResult> ProcessDataElementFunc { get; set; }
        public DataProcessingResult ProcessDataElementResult { get; set; } = DataProcessingResult.Ignored;
        public DataProcessingResult ProcessDataElement(T data, TContext context, CancellationToken cancellationToken)
        {
            return ProcessDataElementFunc?.Invoke(data, context, cancellationToken) ?? ProcessDataElementResult;
        }

        public Action<ILogger, IProgress<int>, CancellationToken> ProcessSourceAction { get; set; }
        public void ProcessSource(ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            ProcessSourceAction?.Invoke(logger, progress, cancellationToken);
        }

        public Action<ISourceDataCooker<T, TContext, TKey>> RegisterSourceDataCookerAction { get; set; }
        public void RegisterSourceDataCooker(ISourceDataCooker<T, TContext, TKey> dataCooker)
        {
            if (RegisterSourceDataCookerAction != null)
            {
                RegisterSourceDataCookerAction.Invoke(dataCooker);
            }
            else
            {
                SourceDataCookers.Add(dataCooker);
            }
        }
    }
}
