// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the different charts that can be used when graphing
    ///     table data.
    /// </summary>
    public enum ChartType
    {
        /// <summary>
        ///     Graph using a line.
        /// </summary>
        Line,

        /// <summary>
        ///     Graph using stacked lines.
        /// </summary>
        StackedLine,

        /// <summary>
        ///     Graph using bars.
        /// </summary>
        StackedBars,

        /// <summary>
        ///     Graph using a state diagram.
        /// </summary>
        StateDiagram,

        /// <summary>
        ///     Graph using points.
        /// </summary>
        PointInTime,

        /// <summary>
        ///     Graph using flames.
        /// </summary>
        Flame,
    }
}
