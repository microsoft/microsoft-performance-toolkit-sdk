// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_3
{
    [DataContract]
    internal class PrebuiltConfigurations
        : PrebuiltConfigurationsBase
    {
        internal static readonly double DTOVersion = 1.3;

        public PrebuiltConfigurations()
        {
            Version = DTOVersion;
        }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }
    }
}
