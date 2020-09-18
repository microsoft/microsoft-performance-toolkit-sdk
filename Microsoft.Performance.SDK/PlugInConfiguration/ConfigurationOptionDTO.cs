// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.PlugInConfiguration
{
    [DataContract]
    internal class ConfigurationOptionDTO
    {
        /// <summary>
        /// The configuration option name.
        /// </summary>
        [DataMember]
        internal string Name { get; set; }

        /// <summary>
        /// A description of the configuration option.
        /// </summary>
        [DataMember]
        internal string Description { get; set; }

        /// <summary>
        /// The applications which opt-in to this option.
        /// </summary>
        [DataMember]
        internal string[] Applications { get; set; }

        /// <summary>
        /// The runtimes which opt-in to this option.
        /// </summary>
        /// <remarks>
        /// Some runtimes exist as class libraries from which many application may derive. This area is provided for
        /// options which are runtime specific.
        /// </remarks>
        [DataMember]
        internal string[] Runtimes { get; set; }
    }
}
