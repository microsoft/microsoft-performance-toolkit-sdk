// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class UriPluginSource : IPluginSource
    {
        protected UriPluginSource(string name, Uri uri)
        {
            this.Name = name;
            this.Uri = uri;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Uri Uri { get; }
    }
}
