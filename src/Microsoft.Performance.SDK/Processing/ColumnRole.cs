// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines known string values of column roles.
    /// </summary>
    public static class ColumnRole
    {
        /// <summary>
        ///     Indicates the start timestamp.
        /// </summary>
        public const string StartTime = "StartTime";

        /// <summary>
        ///     Indicates the end timestamp.
        /// </summary>
        public const string EndTime = "EndTime";

        /// <summary>
        ///     Indicates duration in time.
        /// </summary>
        public const string Duration = "Duration";

        /// <summary>
        ///     How to partition the data across physical resources.
        /// </summary>
        public const string ResourceId = "ResourceId";

        /// <summary>
        ///     Indicates the X Left Top of the Rectangle.
        /// </summary>
        public const string RecLeft = "RecLeft";

        /// <summary>
        ///     Indicates the Y Left Top of the Rectangle.
        /// </summary>
        public const string RecTop = "RecTop";

        /// <summary>
        ///     Indicates the Height of the Rectangle.
        /// </summary>
        public const string RecHeight = "RecHeight";

        /// <summary>
        ///     Indicates the Width of the Rectangle.
        /// </summary>
        public const string RecWidth = "RecWidth";
    }
}
