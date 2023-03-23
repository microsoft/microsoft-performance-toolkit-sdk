// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Exception that occurs when the plugin package cannot be created.
    /// </summary>
    public class PluginPackageCreationException
       : PluginPackageException
    {
        public PluginPackageCreationException()
        {
        }

        public PluginPackageCreationException(string message)
            : base(message)
        {
        }

        public PluginPackageCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
