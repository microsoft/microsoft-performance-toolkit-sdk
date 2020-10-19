// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the role of a column in a graph.
    /// </summary>
    public enum ColumnRole
    {
        /// <summary>
        ///     Indicates the thread ID of the row at the same point as the start time.
        /// </summary>
        StartThreadId = 0,

        /// <summary>
        ///     The thread ID of the row at the same point as end time.
        /// </summary>
        EndThreadId,

        /// <summary>
        ///     Indicates the start timestamp for the x-axis.
        /// </summary>
        StartTime,

        /// <summary>
        ///     Indicates the end timestamp for the x-axis.
        /// </summary>
        EndTime,

        /// <summary>
        ///     Indicates how long each row takes on the time line.
        /// </summary>
        Duration,

        /// <summary>
        ///     Hierarchical Time Tree. Used for Wait Analysis.
        /// </summary>
        HierarchicalTimeTree,

        /// <summary>
        ///     How to partition the data across physical resources.
        /// </summary>
        ResourceId,

        /// <summary>
        ///     Indicates how long each row waited on the time line.
        /// </summary>
        WaitDuration,

        /// <summary>
        ///     Indicates the end timestamp for waiting on the x-axis.
        /// </summary>
        WaitEndTime,

        /// <summary>
        ///     Indicates the X Left Top of the Rec.
        /// </summary>
        RecLeft,

        /// <summary>
        ///     Indicates the Y Left Top of the Rec.
        /// </summary>
        RecTop,

        /// <summary>
        ///     Indicates the Height of the Rec.
        /// </summary>
        RecHeight,

        /// <summary>
        ///     Indicates the Width of the Rec.
        /// </summary>
        RecWidth,

        /// <summary>
        ///     The count of the number of roles in this enumeration.
        ///     This value is always last in the enumeration and should
        ///     not actually be used.
        /// </summary>
        CountColumnMetadata
    }
}
