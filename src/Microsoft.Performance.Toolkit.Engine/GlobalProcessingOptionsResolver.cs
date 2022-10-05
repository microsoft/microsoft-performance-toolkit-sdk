// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing.Options
{
    internal class GlobalProcessingOptionsResolver : IProcessingOptionsResolver
    {
        private readonly ProcessorOptions processorOptions;

        /// <summary>
        ///     Gets the instance that represents the default <see cref="GlobalProcessingOptionsResolver"/>.
        /// </summary>
        public static readonly GlobalProcessingOptionsResolver Default
            = new GlobalProcessingOptionsResolver(ProcessorOptions.Default);

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
        /// <exception cref="ArgumentNullException">
        ///     The specified <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        public ProcessorOptions GetProcessorOptions(IDataSourceGroup dataSourceGroup, IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));
            return this.processorOptions;
        }
    }
}
