// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;

namespace Microsoft.Performance.SDK.ColumnCommands;

/// <summary>
///     The result of one source code download attempted by a
///     <see cref="DownloadSourceCodeCommand"/>. On success, exposes a
///     <see cref="Uri"/> that points to the downloaded source code (typically
///     a local file URI) that the host can open using the appropriate platform
///     mechanism.
/// </summary>
public class DownloadSourceCodeResult
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="DownloadSourceCodeResult"/> class representing a
    ///     successful download.
    /// </summary>
    /// <param name="uri">
    ///     The URI pointing to the downloaded source code that the host
    ///     should open.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="uri"/> is <c>null</c>.
    /// </exception>
    public DownloadSourceCodeResult(Uri uri)
    {
        Guard.NotNull(uri, nameof(uri));

        this.Uri = uri;
        this.Success = true;
    }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="DownloadSourceCodeResult"/> class representing a
    ///     failure. <see cref="Success"/> will be <c>false</c>.
    /// </summary>
    /// <param name="errorMessage">
    ///     A human-readable message describing why the source code could
    ///     not be downloaded.
    /// </param>
    /// <param name="uri">
    ///     An optional URI associated with the failure. Because this
    ///     constructor represents a failure case, this URI does not refer
    ///     to a successfully downloaded local resource. When not
    ///     <c>null</c>, it typically represents the remote URI that
    ///     corresponds to the row value (for example, the source location
    ///     the command attempted to download from), which the host may
    ///     choose to surface to the user as a fallback.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="errorMessage"/> is <c>null</c>.
    /// </exception>
    public DownloadSourceCodeResult(string errorMessage, Uri? uri)
    {
        Guard.NotNull(errorMessage, nameof(errorMessage));

        this.ErrorMessage = errorMessage;
        Uri = uri;
        this.Success = false;
    }

    /// <summary>
    ///     Gets a value indicating whether this download attempt completed
    ///     successfully and <see cref="Uri"/> is safe to open. When
    ///     <c>false</c>, hosts should not attempt to open <see cref="Uri"/>
    ///     and should surface <see cref="ErrorMessage"/> instead.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    ///     Gets an optional human-readable error message describing why
    ///     the source code could not be downloaded, or <c>null</c> when
    ///     no error occurred.
    /// </summary>
    public string? ErrorMessage { get; } = null;

    /// <summary>
    ///     Gets the URI associated with this result. When
    ///     <see cref="Success"/> is <c>true</c>, this is the URI of the
    ///     downloaded source code (typically a local file URI) that the
    ///     host should open. When <see cref="Success"/> is <c>false</c>,
    ///     this value may be <c>null</c> or may be the remote URI
    ///     corresponding to the row value that the command attempted to
    ///     download from; in the failure case hosts should not treat it as
    ///     a successfully downloaded local resource.
    /// </summary>
    public Uri? Uri { get; }
}
