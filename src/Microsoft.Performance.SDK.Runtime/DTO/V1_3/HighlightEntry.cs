// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class HighlightEntry
    {

        [DataMember(Order = 1)]
        public ColumnIdentifier StartTimeColumnIdentifier { get; set; }


        [DataMember(Order = 2)]
        public ColumnIdentifier EndTimeColumnIdentifier { get; set; }


        [DataMember(Order = 3)]
        public ColumnIdentifier DurationColumnIdentifier { get; set; }


        [DataMember(Order = 4)]
        public string HighlightQuery { get; set; }


        [DataMember(Order = 5)]
        public Color HighlightColor { get; set; }
    }
}
