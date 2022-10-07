// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Diagnostics;

namespace Microsoft.Performance.Toolkit.Engine
{
    internal class GlobalProcessingOptionsResolver 
        : IProcessingOptionsResolver
    {
        private readonly ProcessorOptions processorOptions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalProcessingOptionsResolver"/> class.
        /// </summary>
        /// <param name="options"> 
        ///     The <see cref="ProcessorOptions"/> to return from all calls to <see cref="GetProcessorOptions"/>, 
        ///     regardless of the specified <see cref="Guid"/> and <see cref="IDataSourceGroup "/>. 
        /// </param>
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
