// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     A <see cref="IPluginSource"/> specified with a <see cref="Uri"/>.
    /// </summary>
    public class UriPluginSource : IPluginSource
    {
        protected UriPluginSource(string name, Uri uri)
        {
            this.Name = name;
            this.Uri = uri;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        ///     Gets the <see cref="Uri"/> of this source.
        /// </summary>
        public Uri Uri { get; }
    }
}
