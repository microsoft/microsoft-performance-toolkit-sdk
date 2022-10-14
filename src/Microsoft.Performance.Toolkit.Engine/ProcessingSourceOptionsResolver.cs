// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.Toolkit.Engine
{
    internal sealed class ProcessingSourceOptionsResolver 
        : IProcessingOptionsResolver
    {
        private readonly IDictionary<Guid, ProcessorOptions> processingSourceToOptionsMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceOptionsResolver"/> class.
        /// </summary>
        /// <param name="optionsMap"> A dictionary which maps <see cref="ProcessorOptions"/> for a given processor which will process the data sources. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="optionsMap"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceOptionsResolver(IDictionary<Guid, ProcessorOptions> optionsMap)
        {
            Guard.NotNull(optionsMap, nameof(optionsMap));
            this.processingSourceToOptionsMap = optionsMap;
        }

        /// <summary>
        ///     Return <see cref="ProcessorOptions"/> for a ProcessingSource.
        /// </summary>
        /// <remarks> 
        ///     If a <paramref name="processingSourceGuid"/> wasn't specified, we return the <see cref="ProcessorOptions.Default"/>. 
        /// </remarks>
        /// <param name="processingSourceGuid"> A Guid for a  processing source to find processor options.</param>
        /// <param name="dataSourceGroup">A DataSourceGroup. This is ignored in this implementation. </param>
        /// <returns>Options for a given ProcessingSource.</returns>
        /// <exception cref="ArgumentNullException">
        ///     The specified <paramref name="processingSourceGuid"/> is <c>null</c>.
        /// </exception>
        public ProcessorOptions GetProcessorOptions(Guid processingSourceGuid, IDataSourceGroup dataSourceGroup)
        {
            Debug.Assert(dataSourceGroup != null);
            return GetProcessorOptions(processingSourceGuid);
        }

        /// <summary>
        ///     Return options for processing for a ProcessingSource.
        /// </summary>
        /// <remarks> 
        ///     If a <paramref name="processingSourceGuid"/> wasn't specified, we return the <see cref="ProcessorOptions.Default"/>. 
        /// </remarks>
        /// <param name="processingSourceGuid"> A Guid for a  processing source to find processor options.</param>
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
