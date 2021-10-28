// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.PluginConfiguration
{
    [DataContract(Name = "PlugInConfigurationDTO")]
    internal class PluginConfigurationDTO
    {
        /// <summary>
        /// Name of the plugin to which this configuration belongs.
        /// </summary>
        [DataMember(Order = 0, Name = "PlugInName")]
        internal string PluginName { get; set; }

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
