// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Base class for all SDK extensibility exceptions.
    /// </summary>
    public class ExtensibilityException : Exception
    {
        public ExtensibilityException(string message)
            : base(message)
        {
        }

        public ExtensibilityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     There is a problem with a table extension.
    /// </summary>
    public class ExtendedTableException : ExtensibilityException
    {
        public ExtendedTableException(string message)
            : base(message)
        {
        }
    }
}
