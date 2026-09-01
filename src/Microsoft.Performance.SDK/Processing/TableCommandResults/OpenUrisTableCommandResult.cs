// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     A <see cref="TableCommandResult"/> that instructs the host to open one or
///     more <see cref="Uri"/>s on the plugin's behalf. The interpretation of each
///     <see cref="Uri"/> (for example, launching a web browser for <c>http(s)</c>
///     URIs, opening a file for <c>file</c> URIs, or handing off to a registered
///     custom scheme handler) is left to the host.
/// </summary>
/// <param name="Uris">
///     The <see cref="Uri"/>s to open. Must not be <c>null</c>.
/// </param>
public sealed record OpenUrisTableCommandResult : TableCommandResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OpenUrisTableCommandResult"/>
    ///     record.
    /// </summary>
    /// <param name="Uris">
    ///     The <see cref="Uri"/>s to open. Must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="Uris"/> is <c>null</c>.
    /// </exception>
    public OpenUrisTableCommandResult(IReadOnlyList<Uri> Uris)
    {
        Guard.NotNull(Uris, nameof(Uris));
        this.Uris = Uris;
    }

    /// <summary>
    ///     Gets the <see cref="Uri"/>s the host should open.
    /// </summary>
    public IReadOnlyList<Uri> Uris { get; init; }
}
