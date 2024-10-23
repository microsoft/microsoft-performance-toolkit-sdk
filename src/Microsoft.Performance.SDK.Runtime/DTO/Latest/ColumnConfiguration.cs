// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.Latest
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
        ///     The unique identifier of the column variant to use.
        /// </summary>
        [DataMember]
        public Guid? VariantGuid { get; set;  }

        /// <summary>
        ///     UI hints for displaying the column.
        /// </summary>
        [DataMember]
        public UIHints DisplayHints { get; set;  }
    }
}
