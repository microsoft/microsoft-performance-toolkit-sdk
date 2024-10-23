// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO.V1_3;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class ColumnRoleEntry
        : ISupportUpgrade<V1_3.ColumnIdentifier>
    {
        [DataMember(Order = 1)]
        public Guid ColumnGuid { get; set; }

        [DataMember(Order = 2)]
        public string ColumnName { get; set; }

        public ColumnIdentifier Upgrade(ILogger logger)
        {
            return new ColumnIdentifier()
            {
                ColumnGuid = ColumnGuid,
            };
        }
    }
}
