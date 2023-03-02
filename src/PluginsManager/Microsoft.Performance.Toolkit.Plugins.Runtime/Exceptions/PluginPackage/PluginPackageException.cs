// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Base exception type for the PluginPackage.
    /// </summary>
    public abstract class PluginPackageException
        : PluginsManagerException
    {
        protected PluginPackageException()
        {
        }

        protected PluginPackageException(string message)
            : base(message)
        {
        }
        
        protected PluginPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
