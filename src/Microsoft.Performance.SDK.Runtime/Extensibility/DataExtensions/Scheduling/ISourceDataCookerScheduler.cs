// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling
{
    internal interface ISourceDataCookerScheduler
    {
        SchedulingPass Pass0 { get; }

        DataCookerSchedulingNode GetSchedulingNode(DataCookerPath dataCookerPath);
    }
}
