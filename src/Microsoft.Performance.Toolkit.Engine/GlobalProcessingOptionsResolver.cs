// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing.Options
{
    internal class GlobalProcessingOptionsResolver 
        : IProcessingOptionsResolver
    {
        private readonly ProcessorOptions processorOptions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalProcessingOptionsResolver"/> class.
        /// </summary>
        /// <param name="options"> Standard <see cref="ProcessorOptions"/> for all ProcessingSources and DataSourceGroups. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public GlobalProcessingOptionsResolver(ProcessorOptions options)
        {
            Guard.NotNull(options, nameof(options));
            processorOptions = options;
        }

        /// <inheritdoc/>
        public ProcessorOptions GetProcessorOptions(Guid processingSourceGuid, IDataSourceGroup dataSourceGroup)
        {
            Debug.Assert(processingSourceGuid != null);
            Debug.Assert(dataSourceGroup != null);

            return this.processorOptions;
        }
    }
}
