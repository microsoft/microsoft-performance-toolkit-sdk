// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     Exception that occurs when the plugin package is malformed.
    /// </summary>
    internal class MalformedPluginPackageException
       : PluginPackageException
    {
        public MalformedPluginPackageException()
        {
        }

        public MalformedPluginPackageException(string message)
            : base(message)
        {
        }

        public MalformedPluginPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
