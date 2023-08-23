// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    public abstract class ConsoleAppException
        : Exception
    {
        protected ConsoleAppException()
        {
        }

        protected ConsoleAppException(string message)
            : base(message)
        {
        }

        protected ConsoleAppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
