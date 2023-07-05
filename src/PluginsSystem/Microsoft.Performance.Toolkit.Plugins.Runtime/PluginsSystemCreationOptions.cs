// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Options for creating a <see cref="PluginsSystem"/>.
    /// </summary>
    public class PluginsSystemCreationOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether or not plugins SDK versions should
        ///     be checked for compatibility with the running SDK. The default value is
        ///     <c>true</c>.
        /// </summary>a
        public bool ValidateSdkVersion { get; set; } = true;

        /// <summary>
        ///     Gets or sets additional <see cref="IPluginValidator"/>s that will be used to validate
        ///     plugins during plugin discovery, installation, and loading.
        /// </summary>
        public IReadOnlyCollection<IPluginValidator> AdditionalValidators { get; set; } =
            Array.Empty<IPluginValidator>();

        /// <summary>
        ///     Gets or sets an <see cref="IInvalidPluginsGate"/> to use when determining whether
        ///     to continue a plugins system operation despite plugin validation errors.
        ///     If a value is not set, operations will *never* continue if validation errors occur.
        /// </summary>
        public IInvalidPluginsGate InvalidPluginsGate { get; set; } = new NeverPassInvalidPluginsGate();
    }
}
