// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Enumerates the message types that can be displayed to the user.
    ///     See <see cref="IApplicationEnvironment.DisplayMessage(MessageType, System.IFormatProvider, string, object[])"/>.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        ///     The message is informational.
        /// </summary>
        Information,

        /// <summary>
        ///     The message is a warning.
        /// </summary>
        Warning,

        /// <summary>
        ///     The message is an error.
        /// </summary>
        Error,
    };
}
