// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    public sealed class DiscovererSourceCollection
    {
        private readonly Dictionary<Type, IList<IPluginDiscovererSource>> discovererSourcesMap;

        public DiscovererSourceCollection ()
        {
            this.discovererSourcesMap = new Dictionary<Type, IList<IPluginDiscovererSource>>();
        }

        public void Put<T>(IPluginDiscovererSource<T> item) where T : class, IPluginSource
        {
            Type type = typeof(T);
            if (!this.discovererSourcesMap.TryGetValue(type, out IList<IPluginDiscovererSource> discovererSources))
            {
                discovererSources = new List<IPluginDiscovererSource>();
                this.discovererSourcesMap[type] = discovererSources;
            }

            discovererSources.Add(item);

        }

        public IEnumerable<IPluginDiscovererSource<T>> Get<T>() where T : class, IPluginSource
        {
            return this.discovererSourcesMap[typeof(T)] as IList<IPluginDiscovererSource<T>>;
        }

        public IEnumerable<IPluginDiscovererSource> AllPluginDiscovererSources
        {
            get
            {
                return this.discovererSourcesMap.Values.SelectMany(x => x);
            }
        }
    }
}
