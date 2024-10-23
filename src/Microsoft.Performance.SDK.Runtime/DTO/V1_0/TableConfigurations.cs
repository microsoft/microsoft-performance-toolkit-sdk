// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class TableConfigurations
        : ISupportUpgrade<V1_3.TableConfigurations>
    {
        [DataMember(Order = 1)]
        public Guid TableId { get; set; }

        [DataMember(Order = 2)]
        public string DefaultConfigurationName { get; set; }

        [DataMember(Order = 3)]
        public TableConfiguration[] Configurations { get; set; }

        public V1_3.TableConfigurations Upgrade()
        {
            return new V1_3.TableConfigurations()
            {
                TableId = this.TableId,
                DefaultConfigurationName = this.DefaultConfigurationName,
                Configurations = this.Configurations.Select(config => config.Upgrade()).ToArray()
            };
        }
    }
}
