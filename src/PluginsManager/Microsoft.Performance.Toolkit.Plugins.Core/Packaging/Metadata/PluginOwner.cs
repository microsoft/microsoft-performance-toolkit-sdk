// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Gets or sets the information of a plugin owner.
    /// </summary>
    public sealed class PluginOwner
    {
        /// <summary>
        ///     Gets or sets the name of the plugin owner.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the address of the owner, if any.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     Gets or sets the email addresses of the owner, if any.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }

        /// <summary>
        ///     Gets or sets the phone numbers of the owner, if any.
        /// </summary>
        public IEnumerable<string> PhoneNumbers { get; set; }
    }
}
