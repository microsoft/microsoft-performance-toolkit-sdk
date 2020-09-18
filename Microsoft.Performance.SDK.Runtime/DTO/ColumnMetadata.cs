// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class ColumnMetadata
    {
        /// <summary>
        /// Column identifier.
        /// </summary>
        [DataMember]
        public Guid Guid { get; set; }

        /// <summary>
        /// Column name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Short Description of the column.
        /// </summary>
        [DataMember]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Description of the column.
        /// </summary>
        [DataMember]
        public string Description { get; set; }
    }
}
