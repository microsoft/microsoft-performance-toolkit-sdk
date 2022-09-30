// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.DataSourceGrouping;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    // todo : refactor with IDataSourceGroup
    public interface IProcessingOptionsResolver
    {

        /// <summary>
        ///     Mapping of DataSourceGroups to <see cref="ProcessorOptions"/>.
        /// </summary>
        IDictionary<IEnumerable<IDataSource>, ProcessorOptions> OptionsForDataSourceGroups { get; }

        /// <summary>
        ///     Get the processor options for a group of data sources processed by a given processor.
        /// </summary>
        /// <param name="dataSourceGroup"> A group of data sources </param>
        /// <param name="processingSource"> a processing source to process the data sources. </param>
        /// <returns></returns>
        ProcessorOptions GetProcessorOptions(IEnumerable<IDataSource> dataSourceGroup, IProcessingSource processingSource);
    }
}

