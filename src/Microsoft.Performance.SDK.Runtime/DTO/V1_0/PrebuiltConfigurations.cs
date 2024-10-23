// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class PrebuiltConfigurations
        : PreviousPrebuiltConfigurationBase<Latest.PrebuiltConfigurations>
    {
        internal static readonly double DTOVersion = 1.0;

        public PrebuiltConfigurations()
        {
            this.Version = DTOVersion;
        }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }

        protected override Latest.PrebuiltConfigurations UpgradeToNext()
        {
            return new Latest.PrebuiltConfigurations()
            {
                Tables = this.Tables.Select(configs => configs.Upgrade()).ToArray()
            };
        }
    }
}