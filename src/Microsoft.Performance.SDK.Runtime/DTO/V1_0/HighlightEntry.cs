// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Drawing;
using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class HighlightEntry
        : ISupportUpgrade<V1_3.HighlightEntry>
    {
        [DataMember(Order = 1)]
        public string StartTimeColumnName { get; set; }

        [DataMember(Order = 2)]
        public Guid StartTimeColumnGuid { get; set; }

        [DataMember(Order = 3)]
        public string EndTimeColumnName { get; set; }

        [DataMember(Order = 4)]
        public Guid EndTimeColumnGuid { get; set; }

        [DataMember(Order = 5)]
        public string DurationColumnName { get; set; }

        [DataMember(Order = 6)]
        public Guid DurationColumnGuid { get; set; }

        [DataMember(Order = 7)]
        public string HighlightQuery { get; set; }

        [DataMember(Order = 8)]
        public Color HighlightColor { get; set; }

        public V1_3.HighlightEntry Upgrade(ILogger logger)
        {
            return new V1_3.HighlightEntry()
            {
                StartTimeColumnIdentifier = new V1_3.ColumnIdentifier() {  ColumnGuid = this.StartTimeColumnGuid, },
                EndTimeColumnIdentifier = new V1_3.ColumnIdentifier() { ColumnGuid = this.EndTimeColumnGuid, },
                DurationColumnIdentifier = new V1_3.ColumnIdentifier() { ColumnGuid = this.DurationColumnGuid, },
                HighlightQuery = this.HighlightQuery,
                HighlightColor = this.HighlightColor,
            };
        }
    }
}
