// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class ColumnConfiguration
    {
        /// <summary>
        ///     Metadata describing the column.
        /// </summary>
        [DataMember]
        public ColumnMetadata Metadata { get; set;  }

        /// <summary>
        ///     UI hints for displaying the column.
        /// </summary>
        [DataMember]
        public UIHints DisplayHints { get; set;  }
    }
}
