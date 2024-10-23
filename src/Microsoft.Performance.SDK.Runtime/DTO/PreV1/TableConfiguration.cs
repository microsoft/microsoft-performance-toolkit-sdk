// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO.Enums;
using Microsoft.Performance.SDK.Runtime.DTO.V1_0;

namespace Microsoft.Performance.SDK.Runtime.DTO.PreV1
{
    [DataContract]
    internal class TableConfiguration
        : ISupportUpgrade<V1_0.TableConfiguration>
    {
        /// <summary>
        ///     The table name.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        ///     The layout style for the table.
        /// </summary>
        [DataMember(Order = 2)]
        public TableLayoutStyle Layout { get; set; }

        /// <summary>
        ///     The type of chart displayed for this table.
        /// </summary>
        [DataMember(Order = 3)]
        public Enums.ChartType ChartType { get; set; }

        /// <summary>
        ///     Gets or sets the aggregation over time mode for this table.
        /// </summary>
        [DataMember(Order = 4)]
        public Enums.AggregationOverTime AggregationOverTime { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial filtering in this table.
        /// </summary>
        [DataMember(Order = 5)]
        public string InitialFilterQuery { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial expansion in this table.
        /// </summary>
        [DataMember(Order = 6)]
        public string InitialExpansionQuery { get; set; }

        /// <summary>
        ///     Gets or sets the query for initial selection in this table.
        /// </summary>
        [DataMember(Order = 7)]
        public string InitialSelectionQuery { get; set; }

        /// <summary>
        ///     Get or sets whether the initial filter should be kept in this table.
        /// </summary>
        [DataMember(Order = 8)]
        public bool InitialFilterShouldKeep { get; set; }

        /// <summary>
        ///     Gets or sets the top value of the graph filter in this value.
        /// </summary>
        [DataMember(Order = 9)]
        public int GraphFilterTopValue { get; set; }

        /// <summary>
        ///     Gets or sets the threshold value of the graph filter in this table.
        /// </summary>
        [DataMember(Order = 10)]
        public double GraphFilterThresholdValue { get; set; }

        /// <summary>
        ///     Gets or sets the name of the column for graph filtering.
        /// </summary>
        [DataMember(Order = 11)]
        public string GraphFilterColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the column for graph filtering.
        /// </summary>
        [DataMember(Order = 12)]
        public Guid GraphFilterColumnGuid { get; set; }

        /// <summary>
        ///     Gets or sets an RTF string that is used to show the help information for this table.
        /// </summary>
        [DataMember(Order = 13)]
        public string HelpText { get; set; }

        /// <summary>
        ///     Gets or sets the collection of query entries that are used to highlight in this table.
        /// </summary>
        [DataMember(Order = 14)]
        public IEnumerable<V1_0.HighlightEntry> HighlightEntries { get; set; }

        /// <summary>
        ///     Columns that may appear in the table.
        /// </summary>
        [DataMember(Order = 15)]
        public IEnumerable<V1_0.ColumnConfiguration> Columns { get; set; }

        /// <summary>
        ///     The roles and their associated column entries.
        /// </summary>
        [DataMember(Order = 16)]
        public IDictionary<ColumnRole, ColumnRoleEntry> ColumnRoles { get; set; }

        public V1_0.TableConfiguration Upgrade(ILogger logger)
        {
            var newColumnRoles = new Dictionary<string, ColumnRoleEntry>(this.ColumnRoles.Count);

            foreach (var columnRolePair in this.ColumnRoles)
            {
                var role = columnRolePair.Key;
                var entry = columnRolePair.Value;

                string newEntry = null;

                switch(role)
                {
                    case ColumnRole.Invalid:
                    case ColumnRole.CountColumnMetadata:
                        newEntry = null;
                        break;

                    // These column roles are no longer defined, converting to a string for backwards compatibility.
                    // See ColumnRole summary for details.
                    case ColumnRole.StartThreadId:
                    case ColumnRole.EndThreadId:
                    case ColumnRole.HierarchicalTimeTree:
                    case ColumnRole.WaitDuration:
                    case ColumnRole.WaitEndTime:
                    case ColumnRole.RecLeft:
                    case ColumnRole.RecTop:
                    case ColumnRole.RecHeight:
                    case ColumnRole.RecWidth:
                        newEntry = role.ToString();
                        break;

                    case ColumnRole.StartTime:
                        newEntry = Processing.ColumnRole.StartTime;
                        break;

                    case ColumnRole.EndTime:
                        newEntry = Processing.ColumnRole.EndTime;
                        break;

                    case ColumnRole.Duration:
                        newEntry = Processing.ColumnRole.Duration;
                        break;

                    case ColumnRole.ResourceId:
                        newEntry = Processing.ColumnRole.ResourceId;
                        break;
                }

                if (newEntry != null)
                {
                    newColumnRoles.Add(newEntry, entry);
                }                
            }

            return new DTO.V1_0.TableConfiguration()
            {
                Name = this.Name,
                ChartType = this.ChartType,
                AggregationOverTime = this.AggregationOverTime,
                InitialFilterQuery = this.InitialFilterQuery,
                InitialExpansionQuery = this.InitialExpansionQuery,
                InitialSelectionQuery = this.InitialSelectionQuery,
                InitialFilterShouldKeep = this.InitialFilterShouldKeep,
                GraphFilterTopValue = this.GraphFilterTopValue,
                GraphFilterThresholdValue = this.GraphFilterThresholdValue,
                GraphFilterColumnName = this.GraphFilterColumnName,
                GraphFilterColumnGuid = this.GraphFilterColumnGuid,
                Description = this.HelpText,
                HighlightEntries = this.HighlightEntries,
                Columns = this.Columns,
                ColumnRoles = newColumnRoles
            };
        }
    }
}
