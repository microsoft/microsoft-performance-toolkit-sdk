// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class PrebuiltConfigurations
    {
        internal static readonly double DTOVersion = 0.1;

        public PrebuiltConfigurations()
        {
            this.Version = DTOVersion;
        }

        [DataMember(Order = 1)]
        public double Version { get; set; }

        [DataMember(Order = 2)]
        public TableConfigurations[] Tables { get; set; }
    }
}
