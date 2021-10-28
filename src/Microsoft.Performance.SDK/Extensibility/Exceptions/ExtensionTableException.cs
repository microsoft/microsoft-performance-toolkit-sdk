// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.Exceptions
{
    /// <summary>
    ///     Represents an exception with an extension table.
    /// </summary>
    public class ExtensionTableException
        : ExtensibilityException
    {
        /// <inheritdoc/>
        public ExtensionTableException(string message)
            : base(message)
        {
        }
    }
}
