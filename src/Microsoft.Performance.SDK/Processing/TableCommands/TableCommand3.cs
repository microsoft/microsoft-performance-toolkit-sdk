// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     The non-generic base for all table commands surfaced through
    ///     <see cref="ITableBuilder"/>. This base carries the metadata needed to
    ///     store heterogeneous commands in a single collection. Hosts should
    ///     inspect the runtime type of each command to determine whether they
    ///     recognize (and can execute) it. Unknown command types may be safely
    ///     ignored.
    /// </summary>
    public abstract class TableCommand3
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableCommand3"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The human-readable name of the command. Leading and trailing whitespace
        ///     is trimmed. The name must be unique (case-insensitive) within the
        ///     table it is registered on.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="commandName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="commandName"/> is empty or whitespace.
        /// </exception>
        protected TableCommand3(string commandName)
        {
            Guard.NotNullOrWhiteSpace(commandName, nameof(commandName));

            this.CommandName = commandName.Trim();
        }

        /// <summary>
        ///     Gets the human-readable name of the command.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        ///     Gets the type of the context object that the host must supply when
        ///     invoking this command. Hosts should use this to determine whether they
        ///     can provide a compatible context before attempting to execute the
        ///     command.
        /// </summary>
        public abstract Type ContextType { get; }

        /// <summary>
        ///     Gets the type of the result produced by executing this command. Hosts
        ///     should use this to determine how to interpret or consume the value
        ///     returned from the command.
        /// </summary>
        public abstract Type ResultType { get; }
    }
}
