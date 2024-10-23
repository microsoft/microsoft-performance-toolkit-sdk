// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Runtime.DTO.Enums;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class TableConfiguration
    {
        /// <summary>
        ///     The table name.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        ///     The type of chart displayed for this table.
        /// </summary>
        [DataMember(Order = 2)]
        public ChartType ChartType { get; set; }

        /// <summary>
        ///     Gets or sets the aggregation over time mode for this table.
        /// </summary>
        [DataMember(Order = 3)]
        public AggregationOverTime AggregationOverTime { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial filtering in this table.
        /// </summary>
        [DataMember(Order = 4)]
        public string InitialFilterQuery { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial expansion in this table.
        /// </summary>
        [DataMember(Order = 5)]
        public string InitialExpansionQuery { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial selection in this table.
        /// </summary>
        [DataMember(Order = 6)]
        public string InitialSelectionQuery { get; set; }

        /// <summary>
        ///     Get or sets whether the initial filter should be kept in this table.
        /// </summary>
        [DataMember(Order = 7)]
        public bool InitialFilterShouldKeep { get; set; }

        /// <summary>
        ///     Gets or sets the top value of the graph filter in this value.
        /// </summary>
        [DataMember(Order = 8)]
        public int GraphFilterTopValue { get; set; }

        /// <summary>
        ///     Gets or sets the threshold value of the graph filter in this table.
        /// </summary>
        [DataMember(Order = 9)]
        public double GraphFilterThresholdValue { get; set; }

        /// <summary>
        ///     Gets or sets the name of the column for graph filtering.
        /// </summary>
        [DataMember(Order = 10)]
        public string GraphFilterColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the column for graph filtering.
        /// </summary>
        [DataMember(Order = 11)]
        public Guid GraphFilterColumnGuid { get; set; }

        /// <summary>
        ///     Gets or sets an string that is used for information about the table configuration.
        /// </summary>
        [DataMember(Order = 12)]
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the collection of query entries that are used to highlight in this table.
        /// </summary>
        [DataMember(Order = 13)]
        public IEnumerable<HighlightEntry> HighlightEntries { get; set; }

        /// <summary>
        ///     Columns that may appear in the table.
        /// </summary>
        [DataMember(Order = 14)]
        public IEnumerable<ColumnConfiguration> Columns { get; set; }

        /// <summary>
        ///     The roles and their associated column entries.
        /// </summary>
        [DataMember(Order = 15)]
        public IDictionary<string, ColumnRoleEntry> ColumnRoles { get; set; }
    }
}
