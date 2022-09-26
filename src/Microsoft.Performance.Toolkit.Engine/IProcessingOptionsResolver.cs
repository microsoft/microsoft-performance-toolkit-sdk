// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    public interface IProcessingOptionsResolver
    {
        ProcessorOptions GetProcessorOptions(IEnumerable<IDataSource> dataSourceGroup, IProcessingSource processingSource);
    }
}

