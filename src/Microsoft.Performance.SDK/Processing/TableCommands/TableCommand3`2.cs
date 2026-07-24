// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     A strongly-typed table command that receives a
    ///     <typeparamref name="TContext"/> when queried or executed and produces
    ///     a <typeparamref name="TResult"/> when executed.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The type of context supplied to <see cref="CanExecute"/> and
    ///     <see cref="ExecuteAsync"/>. Implementations define the shape of this
    ///     context, allowing commands to receive command-specific state from the
    ///     host.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The type of result produced by <see cref="ExecuteAsync"/>. Use
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
        ///     Determines whether the command can be executed against the supplied
        ///     <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        ///     The command-specific context to evaluate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the command can be executed with the supplied
        ///     <paramref name="context"/>; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CanExecute(TContext context);

        /// <summary>
        ///     Asynchronously executes the command against the supplied
        ///     <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        ///     The command-specific context.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token that may be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A task that completes with the result produced by the command.
        /// </returns>
        public abstract Task<TResult> ExecuteAsync(TContext context, CancellationToken cancellationToken);
    }
}
