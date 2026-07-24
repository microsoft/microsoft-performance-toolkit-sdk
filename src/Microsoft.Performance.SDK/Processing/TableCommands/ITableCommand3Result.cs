// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     Marker interface implemented by types that represent the result
    ///     produced by executing a <see cref="TableCommand3{TContext, TResult}"/>.
    /// </summary>
    /// <remarks>
    ///     Implementations convey any output data or status information that
    ///     the host needs after the command runs. Use
    ///     <see cref="VoidTableCommandResult"/> for commands that do not
    ///     produce a meaningful result.
    /// </remarks>
    public interface ITableCommand3Result;
}
