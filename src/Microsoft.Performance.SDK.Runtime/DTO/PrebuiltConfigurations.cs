// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class PrebuiltConfigurations
        : PrebuiltConfigurationsBase
    {
        internal static readonly double DTOVersion = 1;

        public PrebuiltConfigurations()
        {
            this.Version = DTOVersion;
        }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }
    }    
}
