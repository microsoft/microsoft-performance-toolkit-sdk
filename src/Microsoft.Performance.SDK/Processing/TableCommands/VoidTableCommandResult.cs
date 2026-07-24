// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     A sentinel result type used as the <c>TResult</c> for
    ///     <see cref="TableCommand3{TContext, TResult}"/> implementations that
    ///     do not produce a meaningful return value.
    /// </summary>
    public readonly struct VoidTableCommandResult
    {
        /// <summary>
        ///     The single, canonical instance of <see cref="VoidTableCommandResult"/>.
        /// </summary>
        public static readonly VoidTableCommandResult Default = default;
    }
}
