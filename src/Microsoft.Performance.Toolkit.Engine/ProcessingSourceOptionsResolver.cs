﻿using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.Engine
{
    public sealed class ProcessingSourceOptionsResolver : IProcessingOptionsResolver
    {
        private readonly IDictionary<IProcessingSource, ProcessorOptions> processingSourceToOptionsMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceOptionsResolver"/> class.
        /// </summary>
        /// <param name="processingSourceToOptionsMap"> A map for each ProcessingSource to ProcessorOptions</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceOptionsResolver(IDictionary<IProcessingSource, ProcessorOptions> processingSourceToOptionsMap)
        {
            Guard.NotNull(processingSourceToOptionsMap, nameof(processingSourceToOptionsMap));
            this.processingSourceToOptionsMap = processingSourceToOptionsMap;
        }

        /// <summary>
        ///  <see cref="GetProcessorOptions(IProcessingSource)"/>.
        /// </summary>
        /// <param name="dataSourceGroup">A DataSourceGroup</param>
        /// <param name="processingSource">A Processing Source </param>
        /// <returns>ProcessorOptions</returns>
        public ProcessorOptions GetProcessorOptions(IDataSourceGroup dataSourceGroup, IProcessingSource processingSource)
        {
            return GetProcessorOptions(processingSource);
        }

        /// <summary>
        ///     Return <see cref="ProcessorOptions"/> for a given <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="processingSource"> A ProcessingSource to find ProcessingOptions.</param>
        /// <remarks> Verify the ProcessingOptions are valid for the given ProcessingSource.</remarks>
        /// <returns>Options for a given ProcessingSource.</returns>
        /// <exception cref="ArgumentNullException">
        ///     The specified <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     The <c>processorOptions</c> are not supported by the provided <paramref name="processingSource"/>.
        /// </exception>
        private ProcessorOptions GetProcessorOptions(IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));

            this.processingSourceToOptionsMap.TryGetValue(processingSource, out ProcessorOptions processorOptions);

            processorOptions ??= ProcessorOptions.Default;

            // Ensure that the ProcessingSource supports all provided options
            if (!processorOptions.Options.All(o => processingSource.CommandLineOptions.Any(clo => clo.Id.Equals((o.Id as Option)?.Id))))
            {
                throw new NotSupportedException($"All ProcessorOptions are not supported.");
            }

            return processorOptions;
        }
    }
}