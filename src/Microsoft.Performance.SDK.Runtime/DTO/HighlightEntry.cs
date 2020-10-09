// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class HighlightEntry
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
    }
}
