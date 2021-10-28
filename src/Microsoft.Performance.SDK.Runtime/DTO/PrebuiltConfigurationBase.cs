// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    [DataContract]
    internal class PrebuiltConfigurationsBase
    {
        [DataMember(Order = 1)]
        public double Version { get; set; }
    }
}
