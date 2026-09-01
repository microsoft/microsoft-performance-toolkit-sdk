// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     A <see cref="TableCommandResult"/> that carries no value. Use this when a
///     table command completes without needing to return any information to the host.
///     <para/>
///     Prefer the shared <see cref="Instance"/> singleton to avoid allocations.
/// </summary>
public sealed record VoidTableCommandResult : TableCommandResult
{
    /// <summary>
    ///     Gets the shared <see cref="VoidTableCommandResult"/> instance.
    /// </summary>
    public static VoidTableCommandResult Instance { get; } = new VoidTableCommandResult();
}
