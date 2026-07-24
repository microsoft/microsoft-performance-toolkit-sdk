// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.TableCommands;

namespace Microsoft.Performance.SDK.Runtime.TableCommands
{
    /// <summary>
    ///     Wraps the <see cref="Predicate{T}"/> and <see cref="Action{T}"/>
    ///     registered via
    ///     <see cref="ITableBuilder.AddTableCommand2(string, Predicate{TableCommandContext}, Action{TableCommandContext})"/>
    ///     as a <see cref="TableCommand3{TContext, TResult}"/>.
    /// </summary>
    public sealed class TableCommand2Adapter
        : TableCommand3<TableCommandContext, VoidTableCommandResult>
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TableCommand2Adapter"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The name of the command.
        /// </param>
        /// <param name="canExecute">
        ///     The predicate used to determine whether the command may execute.
        /// </param>
        /// <param name="onExecute">
        ///     The action invoked to execute the command.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="canExecute"/> is <c>null</c>.
        ///     -or-
        ///     <paramref name="onExecute"/> is <c>null</c>.
        /// </exception>
        public TableCommand2Adapter(
            string commandName,
            Predicate<TableCommandContext> canExecute,
            Action<TableCommandContext> onExecute)
            : base(commandName)
        {
            Guard.NotNull(canExecute, nameof(canExecute));
            Guard.NotNull(onExecute, nameof(onExecute));

            this.CanExecutePredicate = canExecute;
            this.OnExecuteAction = onExecute;
        }

        /// <summary>
        ///     Gets the wrapped can-execute predicate.
        /// </summary>
        public Predicate<TableCommandContext> CanExecutePredicate { get; }

        /// <summary>
        ///     Gets the wrapped on-execute action.
        /// </summary>
        public Action<TableCommandContext> OnExecuteAction { get; }

        /// <inheritdoc />
        public override bool CanExecute(TableCommandContext context)
        {
            return this.CanExecutePredicate(context);
        }

        /// <inheritdoc />
        public override VoidTableCommandResult Execute(TableCommandContext context)
        {
            this.OnExecuteAction(context);
            return VoidTableCommandResult.Default;
        }
    }
}
