// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     Marker interface implemented by types that provide contextual input
    ///     to a <see cref="TableCommand3{TContext, TResult}"/> when it is invoked.
    /// </summary>
    /// <remarks>
    ///     A context carries any data the command needs in order to execute, such
    ///     as the currently selected rows or other state supplied by the host.
    ///     Implementations are typically immutable value or record types.
    /// </remarks>
    public interface ITableCommand3Context;
}
