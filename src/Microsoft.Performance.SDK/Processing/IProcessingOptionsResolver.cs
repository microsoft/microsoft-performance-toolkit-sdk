// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Responsible for choosing the <see cref="ProcessorOptions"/> that will be used for processing.
    /// </summary>
    public interface IProcessingOptionsResolver
    {
        /// <summary>
        ///     Get the processor options for a group of data sources processed by a given processor.
        /// </summary>
        /// <param name="dataSourceGroup"> A group of data sources. </param>
        /// <param name="processingSourceGuid"> Guid for processing source to process the data sources. </param>
        /// <returns>Options <see cref="ProcessorOptions"/> to pass to the <see cref="IProcessingSource"/>.</returns>
        ProcessorOptions GetProcessorOptions(Guid processingSourceGuid, IDataSourceGroup dataSourceGroup);
    }
}

