// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    internal class InvalidManifestException
        : ConsoleAppException
    {
        internal InvalidManifestException()
        {
        }

        internal InvalidManifestException(string message)
            : base(message)
        {
        }

        internal InvalidManifestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
