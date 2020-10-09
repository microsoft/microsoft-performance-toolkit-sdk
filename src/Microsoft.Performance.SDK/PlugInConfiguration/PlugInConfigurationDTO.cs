// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.PlugInConfiguration
{
    [DataContract]
    internal class PlugInConfigurationDTO
    {
        /// <summary>
        /// Name of the plug-in to which this configuration belongs.
        /// </summary>
        [DataMember(Order = 0)]
        internal string PlugInName { get; set; }

        /// <summary>
        /// Configuration file version.
        /// </summary>
        [DataMember(Order = 1)]
        internal string Version { get; set; }

        /// <summary>
        /// Provides a set of options to opt into.
        /// </summary>
        [DataMember]
        internal ConfigurationOptionDTO[] Options { get; set; }
    }
}
