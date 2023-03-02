// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Exception that occurs when the installed plugin is corrupted or missing.
    /// </summary>
    public class InstalledPluginCorruptedOrMissingException
        : InstalledPluginException
    {
        public InstalledPluginCorruptedOrMissingException()
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message)
            : base(message)
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message, InstalledPluginInfo pluginInfo)
            : base(message, pluginInfo)
        {
        }
    }
}
