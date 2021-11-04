// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines a common set of column roles that that are guaranteed to be successfully saved/restored in a serialized <see cref="TableConfiguration" />.
    ///     <br/>Plugins may use any string value defined in this class to communicate column roles to any SDK driver. 
    ///     <br/><br/>If a <see cref="TableConfiguration" /> includes a column role value that is not defined here, it is not guaranteed to:
    ///     <br/>- Be de/serialized correctly
    ///     <br/>- Be understood by an SDK driver
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
    }
}
