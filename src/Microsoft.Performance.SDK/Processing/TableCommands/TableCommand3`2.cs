// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     A strongly-typed table command that receives a
    ///     <typeparamref name="TContext"/> when queried or executed and produces
    ///     a <typeparamref name="TResult"/> when executed.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The type of context supplied to <see cref="CanExecute"/> and
    ///     <see cref="Execute"/>. Implementations define the shape of this
    ///     context, allowing commands to receive command-specific state from the
    ///     host.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The type of result returned by <see cref="Execute"/>. Use
    ///     <see cref="VoidTableCommandResult"/> when the command does not
    ///     produce a value.
    /// </typeparam>
    public abstract class TableCommand3<TContext, TResult>
        : TableCommand3
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TableCommand3{TContext, TResult}"/> class.
        /// </summary>
        /// <param name="commandName">
        ///     The human-readable name of the command.
        /// </param>
        protected TableCommand3(string commandName)
            : base(commandName)
        {
        }

        /// <inheritdoc />
        public override Type ContextType => typeof(TContext);

        /// <inheritdoc />
        public override Type ResultType => typeof(TResult);

        /// <summary>
        ///     Determines whether the command can be executed for the given
        ///     <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        ///     The command-specific context.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <see cref="Execute"/> may be invoked with the
        ///     supplied context; otherwise <c>false</c>.
        /// </returns>
        public abstract bool CanExecute(TContext context);

        /// <summary>
        ///     Executes the command against the supplied
        ///     <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        ///     The command-specific context.
        /// </param>
        /// <returns>
        ///     The result produced by the command.
        /// </returns>
        public abstract TResult Execute(TContext context);
    }
}
