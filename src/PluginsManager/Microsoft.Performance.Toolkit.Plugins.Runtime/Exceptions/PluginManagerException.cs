// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Base exception type for the PluginsManager.
    /// </summary>
    public abstract class PluginsManagerException
       : Exception
    {
        protected PluginsManagerException()
        {
        }

        protected PluginsManagerException(string message)
            : base(message)
        {
        }

        protected PluginsManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
