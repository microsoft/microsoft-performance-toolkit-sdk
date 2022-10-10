// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Responsible for choosing the <see cref="ProcessorOptions"/> that will be used for processing.
    /// </summary>
    public interface IProcessingOptionsResolver
    {
        /// <summary>
        ///     Get the <see cref="ProcessorOptions"/> for an <see cref="IDataSourceGroup"/> processed by a <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="processingSourceGuid"> Guid for a <see cref="IProcessingSource"/> to process the data sources. </param>
        /// <param name="dataSourceGroup"> A group of data sources. </param>
        /// <returns> 
        ///     The <see cref="ProcessorOptions"/> to pass to the <see cref="IProcessingSource"/> 
        ///     with the given <paramref name="processingSourceGuid"/> when it is asked to create an 
        ///     <see cref="ICustomDataProcessor"/> to process the given <paramref name="dataSourceGroup"/>.
        ///     
        ///     It is invalid to return <c>null</c>.
        /// </returns>
        ProcessorOptions GetProcessorOptions(Guid processingSourceGuid, IDataSourceGroup dataSourceGroup);
    }
}

