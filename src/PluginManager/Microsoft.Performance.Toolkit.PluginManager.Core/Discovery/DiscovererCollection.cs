// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    public sealed class DiscovererCollection
    {
        private readonly Dictionary<Type, IPluginDiscovererSource> discoverers;

        public DiscovererCollection ()
        {
            this.discoverers = new Dictionary<Type, IPluginDiscovererSource>();
        }

        public void Put<T>(IPluginDiscovererSource<T> item) where T : class, IPluginSource
        {
            this.discoverers[typeof(T)] = item;
        }

        public IPluginDiscovererSource<T> Get<T>() where T : class, IPluginSource
        {
            return this.discoverers[typeof(T)] as IPluginDiscovererSource<T>;
        }
    }
}
