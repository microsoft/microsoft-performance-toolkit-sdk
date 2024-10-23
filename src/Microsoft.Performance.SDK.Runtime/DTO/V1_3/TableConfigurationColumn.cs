// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class TableConfigurationColumn
    {
        /// <summary>
        ///     The identifier of the column to use.
        /// </summary>
        [DataMember]
        public ColumnIdentifier ColumnIdentifier { get; set;  }

        /// <summary>
        ///     UI hints for displaying the column.
        /// </summary>
        [DataMember]
        public UIHints DisplayHints { get; set;  }
    }
}
