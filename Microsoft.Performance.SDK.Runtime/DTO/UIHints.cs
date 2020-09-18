// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Runtime.DTO.Enums;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class UIHints
    {
        /// <summary>
        /// Column width.
        /// </summary>
        [DataMember]
        public int Width { get; set; }

        /// <summary>
        /// If the column is visible.
        /// </summary>
        [DataMember]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Text alignment for the column.
        /// </summary>
        [DataMember]
        public TextAlignment TextAlignment { get; set; }

        /// <summary>
        /// Determines how a column is sorted.
        /// </summary>
        [DataMember]
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// Column sort priority.
        /// </summary>
        [DataMember]
        public int SortPriority { get; set; }

        /// <summary>
        /// This determines how data from the column will be aggregated in the table when
        /// multiple rows are collapsed.
        /// </summary>
        [DataMember]
        public AggregationMode AggregationMode { get; set; }

        /// <summary>
        /// Determines how a value will be displayed in a cell.
        /// </summary>
        [DataMember]
        public string CellFormat { get; set; }
    }
}
