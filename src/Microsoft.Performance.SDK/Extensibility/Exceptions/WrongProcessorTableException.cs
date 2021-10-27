// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.Exceptions
{
    /// <summary>
    ///     The table is not valid for the given processor.
    /// </summary>
    public class WrongProcessorTableException
        : ExtensionTableException
    {
        /// <inheritdoc/>
        public WrongProcessorTableException(string message)
            : base(message)
        {
        }
    }
}
