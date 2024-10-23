// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.PreV1
{
    [DataContract]
    internal class PrebuiltConfigurations
        : PrebuiltConfigurationsBase,
          ISupportUpgrade<V1_0.PrebuiltConfigurations>
    {
        internal static readonly double DTOVersion = 0.1;

        public PrebuiltConfigurations()
        {
            this.Version = DTOVersion;
        }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }

        public V1_0.PrebuiltConfigurations Upgrade()
        {
            return new V1_0.PrebuiltConfigurations()
            {
                Tables = this.Tables.Select(configs => configs.Upgrade()).ToArray()
            };
        }
    }
}
