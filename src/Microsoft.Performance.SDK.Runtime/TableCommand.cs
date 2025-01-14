// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents a command that can be executed against a table.
    /// </summary>
    public sealed class TableCommand
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableCommand"/>
        ///     class.
        /// </summary>
        /// <param name="menuName">
        ///     The name of the command to be shown to the user.
        /// </param>
        /// <param name="callback">
        ///     The function to execute when the command is invoked.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="menuName"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="menuName"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="callback"/> is <c>null</c>.
        /// </exception>
        public TableCommand(string menuName, TableCommandCallback callback)
        {
            Guard.NotNullOrWhiteSpace(menuName, nameof(menuName));
            Guard.NotNull(callback, nameof(callback));

            this.MenuName = menuName;
            this.Callback = callback;
        }

        /// <summary>
        ///     Gets the name of the command to display to the user.
        /// </summary>
        public string MenuName { get; }

        /// <summary>
        ///     Gets the function to call when the command is invoked.
        /// </summary>
        public TableCommandCallback Callback { get; }
    }
}
