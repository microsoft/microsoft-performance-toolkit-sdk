// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class ColumnIdentifier
    {
        /// <summary>
        ///     The <see cref="Guid"/> of the column to use.
        /// </summary>
        [DataMember]
        public Guid ColumnGuid { get; set; }

        /// <summary>
        ///     The <see cref="Guid"/> of the column's variant to use, if any.
        /// </summary>
        [DataMember]
        public Guid? VariantGuid { get; set; }
    }
}
