// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class ColumnConfiguration
        : ISupportUpgrade<V1_3.TableConfigurationColumn>
    {
        /// <summary>
        ///     Metadata describing the column.
        /// </summary>
        [DataMember]
        public ColumnMetadata Metadata { get; set; }

        /// <summary>
        ///     UI hints for displaying the column.
        /// </summary>
        [DataMember]
        public UIHints DisplayHints { get; set; }

        public V1_3.TableConfigurationColumn Upgrade(Processing.ILogger logger)
        {
            return new V1_3.TableConfigurationColumn()
            {
                ColumnIdentifier = new V1_3.ColumnIdentifier()
                {
                    ColumnGuid = Metadata.Guid,
                },
                DisplayHints = DisplayHints,
            };
        }
    }
}
