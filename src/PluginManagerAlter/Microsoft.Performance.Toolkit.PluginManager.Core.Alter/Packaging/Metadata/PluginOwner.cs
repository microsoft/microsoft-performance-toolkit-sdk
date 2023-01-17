// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Packaging.Metadata
{
    /// <summary>
    /// The information of a plugin owner
    /// </summary>
    public sealed class PluginOwner
    {
        /// <summary>
        /// The name of the plugin owner
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The address of the owner, if any
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The email addresses of the owner, if any
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }

        /// <summary>
        /// The phone numbers of the owner, if any
        /// </summary>
        public IEnumerable<string> PhoneNumbers { get; set; }
    }
}
