// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.DTO.Enums
{
    internal enum ColumnRole
    {
        Invalid = -1,
        StartThreadId = 0,
        EndThreadId,
        StartTime,
        EndTime,
        Duration,
        HierarchicalTimeTree,
        ResourceId,
        WaitDuration,
        WaitEndTime,
        RecLeft,
        RecTop,
        RecHeight,
        RecWidth,
        CountColumnMetadata,
    }
}
