// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    internal class SourceDataCookerDependencyProvider<T, TContext, TKey>
        : ICookedDataRetrieval
        where T : IKeyedDataType<TKey>
    {
        private readonly HashSet<DataCookerPath> availableSourceCookers;
        private readonly ISourceDataCookerRetrieval<T, TContext, TKey> sourceDataCookers;

        internal SourceDataCookerDependencyProvider(
            HashSet<DataCookerPath> availableSourceCookers,
            ISourceDataCookerRetrieval<T, TContext, TKey> sourceDataCookers)
        {
            Debug.Assert(availableSourceCookers != null, $"{nameof(availableSourceCookers)} cannot be null.");
            Debug.Assert(sourceDataCookers != null, $"{nameof(sourceDataCookers)} cannot be null.");

            this.availableSourceCookers = availableSourceCookers;
            this.sourceDataCookers = sourceDataCookers;
        }

        public TOut QueryOutput<TOut>(DataOutputPath dataOutputPath)
        {
            return (TOut)QueryOutput(dataOutputPath);
        }

        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            if(this.availableSourceCookers.Contains(dataOutputPath.CookerPath))
            {
                var cooker = this.sourceDataCookers.GetSourceDataCooker(dataOutputPath.CookerPath);
                return cooker.QueryOutput(dataOutputPath);
            }

            throw new ArgumentException(
                $"The requested data cooker is not available: {dataOutputPath.CookerPath}. " +
                "Consider adding it to the requirements for this data extension.",
                nameof(dataOutputPath));
        }
    }
}
