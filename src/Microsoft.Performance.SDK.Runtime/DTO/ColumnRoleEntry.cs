// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class ColumnRoleEntry
    {
        [DataMember(Order = 1)]
        public Guid ColumnGuid { get; set; }

        [DataMember(Order = 2)]
        public string ColumnName { get; set; }
    }
}
