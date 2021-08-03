// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;

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

        /// <inheritdoc />
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (!this.processorsByParser.TryGetValue(dataOutputPath.SourceParserId, out var processor))
            {
                result = default;
                return false;
            }

            return processor.TryQueryOutput(dataOutputPath, out result);
        }

        /// <inheritdoc />
        public bool TryQueryOutput<TOutput>(DataOutputPath dataOutputPath, out TOutput result)
        {
            if (!this.processorsByParser.TryGetValue(dataOutputPath.SourceParserId, out var processor))
            {
                result = default;
                return false;
            }

            try
            {
                result = processor.QueryOutput<TOutput>(dataOutputPath);
                return true;
            }
            catch (Exception)
            {
            }

            result = default;
            return false;
        }
    }
}
