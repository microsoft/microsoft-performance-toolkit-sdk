// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.PreV1
{
    [DataContract]
    internal class TableConfigurations
        : ISupportUpgrade<DTO.TableConfigurations>
    {
        [DataMember(Order = 1)]
        public Guid TableId { get; set; }

        [DataMember(Order = 2)]
        public string DefaultConfigurationName { get; set; }

        [DataMember(Order = 3)]
        public TableConfiguration[] Configurations { get; set; }

        public DTO.TableConfigurations Upgrade()
        {
            return new DTO.TableConfigurations()
            {
                TableId = this.TableId,
                DefaultConfigurationName = this.DefaultConfigurationName,
                Configurations = this.Configurations.Select(config => config.Upgrade()).ToArray()
            };
        }
    }
}
