// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class ColumnConfiguration
        : ISupportUpgrade<Latest.ColumnConfiguration>
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

        public Latest.ColumnConfiguration Upgrade()
        {
            return new Latest.ColumnConfiguration()
            {
                Metadata = this.Metadata,
                VariantGuid = null,
                DisplayHints = this.DisplayHints,
            };
        }
    }
}