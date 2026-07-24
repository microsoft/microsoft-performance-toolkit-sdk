// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.TableCommands;

namespace Microsoft.Performance.SDK.Runtime.TableCommands
{
    /// <summary>
    ///     Wraps a legacy <see cref="TableCommandCallback"/> registered via
    ///     <see cref="ITableBuilder.AddTableCommand(string, TableCommandCallback)"/>
    ///     as a <see cref="TableCommand3{TContext, TResult}"/>.
    /// </summary>
    /// <remarks>
    ///     <see cref="CanExecute"/> always returns <c>true</c>, matching the
    ///     historical behavior of <c>AddTableCommand</c>.
    /// </remarks>
    public sealed class TableCommandCallbackAdapter
        : TableCommand3<TableCommandContext, VoidTableCommandResult>
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TableCommandCallbackAdapter"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The name of the command.
        /// </param>
        /// <param name="callback">
        ///     The legacy callback to invoke on execution.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="callback"/> is <c>null</c>.
        /// </exception>
        public TableCommandCallbackAdapter(string commandName, TableCommandCallback callback)
            : base(commandName)
        {
            Guard.NotNull(callback, nameof(callback));

            this.Callback = callback;
        }

        /// <summary>
        ///     Gets the wrapped legacy callback.
        /// </summary>
        public TableCommandCallback Callback { get; }

        /// <inheritdoc />
        public override bool CanExecute(TableCommandContext context) => true;

        /// <inheritdoc />
        public override VoidTableCommandResult Execute(TableCommandContext context)
        {
            Guard.NotNull(context, nameof(context));

            this.Callback(context.SelectedRows);
            return VoidTableCommandResult.Default;
        }
    }
}
