// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing.Options
{
    public interface IProcessingOptionsResolver
    {
        /// <summary>
        ///     Get the processor options for a group of data sources processed by a given processor.
        /// </summary>
        /// <param name="dataSourceGroup"> A group of data sources </param>
        /// <param name="processingSource"> a processing source to process the data sources. </param>
        /// <returns>Options <see cref="ProcessorOptions"/> to pass to the <see cref="IProcessingSource"/>.</returns>
        ProcessorOptions GetProcessorOptions(IDataSourceGroup dataSourceGroup, IProcessingSource processingSource);
    }
}

