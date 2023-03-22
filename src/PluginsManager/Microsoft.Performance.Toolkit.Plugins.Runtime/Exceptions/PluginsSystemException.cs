// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Base exception type for the <see cref="Runtime.PluginsSystem"/>.
    /// </summary>
    public abstract class PluginsSystemException
       : Exception
    {
        protected PluginsSystemException()
        {
        }

        protected PluginsSystemException(string message)
            : base(message)
        {
        }

        protected PluginsSystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
