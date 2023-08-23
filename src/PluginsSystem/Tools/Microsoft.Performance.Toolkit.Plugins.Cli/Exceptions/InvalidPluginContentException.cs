// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    public class InvalidPluginContentException
        : ConsoleAppException
    {
        internal InvalidPluginContentException()
        {
        }

        internal InvalidPluginContentException(string message)
            : base(message)
        {
        }

        internal InvalidPluginContentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
