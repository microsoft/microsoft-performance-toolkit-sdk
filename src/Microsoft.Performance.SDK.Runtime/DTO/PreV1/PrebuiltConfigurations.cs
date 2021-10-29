// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.PreV1
{
    [DataContract]
    internal class PrebuiltConfigurations
        : PrebuiltConfigurationsBase,
          ISupportUpgrade<DTO.PrebuiltConfigurations>
    {
        internal static readonly double DTOVersion = 0.1;

        public PrebuiltConfigurations()
        {
            this.Version = DTOVersion;
        }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }

        public DTO.PrebuiltConfigurations Upgrade()
        {
            return new DTO.PrebuiltConfigurations()
            {
                Tables = this.Tables.Select(configs => configs.Upgrade()).ToArray()
            };
        }
    }
}
