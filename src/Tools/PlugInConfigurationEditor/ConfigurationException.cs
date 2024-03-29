// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PluginConfigurationEditor
{
    internal class ConfigurationException
        : Exception
    {
        internal ConfigurationException(string message)
            : base(message)
        {
        }
    }
}
