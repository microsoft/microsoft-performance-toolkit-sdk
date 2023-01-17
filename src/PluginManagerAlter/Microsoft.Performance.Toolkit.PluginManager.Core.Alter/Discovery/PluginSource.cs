// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    /// <summary>
    ///     A source endpoint represented by a URI for discovering plugins.
    /// </summary>
    public sealed class PluginSource
    {
        public PluginSource(Uri uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        ///     Gets the URI of this plugin source.
        /// </summary>
        public Uri Uri { get; }

    }
}
