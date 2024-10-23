// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Drawing;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides information to SDK drivers that allow highlighting/selecting 
    ///     regions of time.
    /// </summary>
    public sealed class HighlightEntryV2
    {
        /// <summary>
        ///      Gets or sets the ID of the start time column.
        /// </summary>
        public ColumnIdentifier StartTimeColumnIdentifier { get; set; }

        /// <summary>
        ///      Gets or sets the ID of the end time column.
        /// </summary>
        public ColumnIdentifier EndTimeColumnIdentifier { get; set; }

        /// <summary>
        ///      Gets or sets the ID of the duration column.
        /// </summary>
        public ColumnIdentifier DurationColumnIdentifier { get; set; }

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
