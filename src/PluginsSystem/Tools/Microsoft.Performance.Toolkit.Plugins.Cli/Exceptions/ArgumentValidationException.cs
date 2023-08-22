// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    public class ArgumentValidationException
        : ConsoleAppException
    {
        internal ArgumentValidationException()
        {
        }

        internal ArgumentValidationException(string message)
            : base(message)
        {
        }

        internal ArgumentValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
