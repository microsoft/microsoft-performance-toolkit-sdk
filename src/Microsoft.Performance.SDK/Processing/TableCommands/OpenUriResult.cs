// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;

namespace Microsoft.Performance.SDK.Processing.TableCommands
{
    /// <summary>
    ///     A table command result that instructs the host to open a URI. Hosts
    ///     are expected to launch <see cref="Uri"/> using the appropriate
    ///     platform mechanism (for example, a web browser for HTTP(S) URIs).
    /// </summary>
    public class OpenUriResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenUriResult"/>
        ///     class.
        /// </summary>
        /// <param name="uri">
        ///     The URI that the host should open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="uri"/> is <c>null</c>.
        /// </exception>
        public OpenUriResult(Uri uri)
        {
            Guard.NotNull(uri, nameof(uri));

            this.Uri = uri;
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
        ///     the command did not produce a usable URI, or <c>null</c> when
        ///     no error occurred.
        /// </summary>
        public string? ErrorMessage { get; } = null;

        /// <summary>
        ///     Gets the URI that the host should open.
        /// </summary>
        public Uri Uri { get; }
    }
}
