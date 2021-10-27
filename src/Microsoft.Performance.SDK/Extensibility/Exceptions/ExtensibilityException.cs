// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility.Exceptions
{
    /// <summary>
    ///     Base class for all SDK extensibility exceptions.
    /// </summary>
    public class ExtensibilityException : Exception
    {
        /// <inheritdoc/>
        public ExtensibilityException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public ExtensibilityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
