// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Exception that occurs when the content of the plugin package failed to be extracted.
    /// </summary>   
    public class PluginPackageExtractionException
        : PluginPackageException
    {
        public PluginPackageExtractionException()
        {
        }

        public PluginPackageExtractionException(string message)
            : base(message)
        {
        }

        public PluginPackageExtractionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
