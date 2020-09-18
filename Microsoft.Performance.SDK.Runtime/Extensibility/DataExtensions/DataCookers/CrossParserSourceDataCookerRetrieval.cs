// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    /// This class wraps a set of source data parsers to make it easy to retrieve data
    /// output from any associated source data cookers.
    /// </summary>
    internal class CrossParserSourceDataCookerRetrieval
        : ICookedDataRetrieval
    {
        private readonly IDictionary<string, ICustomDataProcessorWithSourceParser> processorsByParser 
            = new Dictionary<string, ICustomDataProcessorWithSourceParser>(StringComparer.Ordinal);

        internal CrossParserSourceDataCookerRetrieval(
            IEnumerable<ICustomDataProcessorWithSourceParser> processors)
        {
            Guard.NotNull(processors, nameof(processors));

            foreach (var processor in processors)
            {
                this.processorsByParser.Add(processor.SourceParserId, processor);
            }
        }

        /// <summary>
        /// Retrieves data by name, typecast to the given type.
        /// The caller is coupled to the data extension and expected to know the
        /// data type.
        /// </summary>
        /// <typeparam name="T">Type of the data to retrieve</typeparam>
        /// <param name="dataOutputPath">Unique identifier for the data to retrieve</param>
        /// <returns>The uniquely identified data</returns>
        public T QueryOutput<T>(DataOutputPath dataOutputPath)
        {
            if (!this.processorsByParser.TryGetValue(dataOutputPath.SourceParserId, out var processor))
            {
                throw new ArgumentException($"The specified source parser ({nameof(dataOutputPath.SourceParserId)}) was not found.");
            }

            return processor.QueryOutput<T>(dataOutputPath);
        }

        /// <summary>
        /// Retrieves data by name.
        /// </summary>
        /// <param name="dataOutputPath">Unique identifier for the data to retrieve</param>
        /// <returns>The uniquely identified data, as an object</returns>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            if (!this.processorsByParser.TryGetValue(dataOutputPath.SourceParserId, out var processor))
            {
                throw new ArgumentException($"The specified source parser ({nameof(dataOutputPath.SourceParserId)}) was not found.");
            }

            return processor.QueryOutput(dataOutputPath);
        }
    }
}
