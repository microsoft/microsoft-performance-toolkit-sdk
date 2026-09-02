// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.ColumnCommands;

/// <summary>
///     The result of a <see cref="DownloadSourceCodeCommand"/>. On success,
///     exposes a <see cref="Uri"/> that points to the downloaded source
///     code (typically a local file URI) that the host can open using the
///     appropriate platform mechanism.
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
    ///     to a successfully downloaded local resource. Its meaning, when
    ///     not <c>null</c>, is the attempted download URI.
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
    ///     Gets a value indicating whether the command completed
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
    ///     Gets the URI pointing to the downloaded source code that the
    ///     host should open.
    /// </summary>
    public Uri? Uri { get; }
}


/// Decision: Where to add the column commands.
/// A. Directly to the IDataColumn or IDataColumn&lt;T&gt;
/// B. In the ITableBuilder
/// 
/// Reasons for A:
/// This is column data in much the same way the ColumnConfiguration or the Projection is.
/// 
/// Reasons for B:
/// We've never added data to IColumnData types, but we have added to ITableBuilder.
/// This would follow the same behavior as column variants.
/// 
