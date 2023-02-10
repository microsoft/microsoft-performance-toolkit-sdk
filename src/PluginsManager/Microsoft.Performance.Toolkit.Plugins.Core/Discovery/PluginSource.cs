// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Discovery
{
    /// <summary>
    ///     A URI source endpoint for discovering plugins.
    /// </summary>
    public sealed class PluginSource
    {
        /// <summary>
        ///     Initializes a <see cref="PluginSource"/.
        /// </summary>
        /// <param name="uri">
        ///     A URI that can be used to discover plugins.
        /// </param>
        public PluginSource(Uri uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        ///     Gets the URI to discover plugins from.
        /// </summary>
        public Uri Uri { get; }
    }
}
