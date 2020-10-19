// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Drawing;

namespace Microsoft.Performance.SDK.Processing
{
    public sealed class HighlightEntry
    {
        /// <summary>
        ///     Gets or sets the name of the start time column.
        /// </summary>
        public string StartTimeColumnName { get; set; }

        /// <summary>
        ///      Gets or sets the ID of the start time column.
        /// </summary>
        public Guid StartTimeColumnGuid { get; set; }

        /// <summary>
        ///      Gets or sets the name of the end time column.
        /// </summary>
        public string EndTimeColumnName { get; set; }

        /// <summary>
        ///      Gets or sets the ID of the end time column.
        /// </summary>
        public Guid EndTimeColumnGuid { get; set; }

        /// <summary>
        ///      Gets or sets the name of the duration column.
        /// </summary>
        public string DurationColumnName { get; set; }

        /// <summary>
        ///      Gets or sets the ID of the duration column.
        /// </summary>
        public Guid DurationColumnGuid { get; set; }

        /// <summary>
        ///      Gets or sets a query that specify the highlight.
        /// </summary>
        public string HighlightQuery { get; set; }

        /// <summary>
        ///      Gets or sets the color of the highlight.
        /// </summary>
        public Color HighlightColor { get; set; }
    }
}
