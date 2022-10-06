// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    internal sealed class ProcessingSourceOptionsResolver 
        : IProcessingOptionsResolver
    {
        private readonly IDictionary<Guid, ProcessorOptions> processingSourceToOptionsMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceOptionsResolver"/> class.
        /// </summary>
        /// <param name="processingSourceToOptionsMap"> A map for each ProcessingSource to ProcessorOptions</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="processingSourceToOptionsMap"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceOptionsResolver(IDictionary<Guid, ProcessorOptions> processingSourceToOptionsMap)
        {
            Guard.NotNull(processingSourceToOptionsMap, nameof(processingSourceToOptionsMap));
            this.processingSourceToOptionsMap = processingSourceToOptionsMap;
        }

        /// <summary>
        ///  <see cref="GetProcessorOptions(Guid)"/>.
        /// </summary>
        /// <param name="dataSourceGroup">A DataSourceGroup.</param>
        /// <param name="processingSourceGuid">A Guid for a Processing Source.</param>
        /// <returns> Options for processing per ProcessingSource.</returns>
        public ProcessorOptions GetProcessorOptions(Guid processingSourceGuid, IDataSourceGroup dataSourceGroup)
        {
            return GetProcessorOptions(processingSourceGuid);
        }

        /// <summary>
        ///     Return options for processing for a ProcessingSource.
        /// </summary>
        /// <param name="processingSourceGuid"> A Guid for a  processing source to find processor options.</param>
        /// <remarks> Verify the ProcessingOptions are valid for the given ProcessingSource.</remarks>
        /// <returns>Options for a given ProcessingSource.</returns>
        /// <exception cref="ArgumentNullException">
        ///     The specified <paramref name="processingSourceGuid"/> is <c>null</c>.
        /// </exception>
        private ProcessorOptions GetProcessorOptions(Guid processingSourceGuid)
        {
            Guard.NotNull(processingSourceGuid, nameof(processingSourceGuid));

            var success = processingSourceToOptionsMap.TryGetValue(processingSourceGuid, out ProcessorOptions processorOptions);
            if (!success || processorOptions == null)
            {
                processorOptions = ProcessorOptions.Default;
            }

            return processorOptions;
        }
    }
}
