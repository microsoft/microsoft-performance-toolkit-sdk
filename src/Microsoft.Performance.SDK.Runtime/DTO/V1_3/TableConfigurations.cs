// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class TableConfigurations
    {
        [DataMember(Order = 1)]
        public Guid TableId { get; set; }

        [DataMember(Order = 2)]
        public string DefaultConfigurationName { get; set; }

        [DataMember(Order = 3)]
        public TableConfiguration[] Configurations { get; set; }
    }
}
