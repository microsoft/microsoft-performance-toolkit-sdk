// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public class PluginManagerResourceRepository<T>
        : IPluginManagerResourcesConsumer,
          IPluginManagerResourceRepository<T>
        where T : IPluginManagerResource
    {
        private readonly HashSet<PluginManagerResourceReference> resources;

        /// <summary>
        ///     Initializes a <see cref="PluginManagerResourceRepository{T}"/> with an existing collection of <paramref name="resources"/>.
        /// </summary>
        /// <param name="resources">
        ///     A collection of <see cref="IPluginManagerResource"s of type <see cref="T"/>.
        /// </param>
        public PluginManagerResourceRepository(IEnumerable<T> resources)
        {
            Guard.NotNull(resources, nameof(resources));

            SetResourcesLoggers(resources);
            this.resources = new HashSet<PluginManagerResourceReference>(resources.Select(r => new PluginManagerResourceReference(r)));
        }

        /// <inheritdoc />
        public IEnumerable<T> Resources
        {
            get
            {
                return this.resources.Select(r => r.Instance).OfType<T>();
            }
        }

        /// <inheritdoc />
        public void OnNewResourcesLoaded(IEnumerable<PluginManagerResourceReference> loadedResources)
        {
            Guard.NotNull(loadedResources, nameof(loadedResources));

            lock (this.resources)
            {
                IEnumerable<PluginManagerResourceReference> newResources = loadedResources
                    .Where(r => r.Instance is T)
                    .Except(this.resources);
                
                if (newResources.Any())
                {
                    this.resources.UnionWith(newResources);
                    SetResourcesLoggers(newResources.Select(r => r.Instance).OfType<T>());
                    ResourcesAdded?.Invoke(this, new NewResourcesEventArgs<T>(newResources.Select(r => r.Instance).OfType<T>()));
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<NewResourcesEventArgs<T>> ResourcesAdded;
        
        private void SetResourcesLoggers(IEnumerable<T> pluginManagerResources)
        {
            foreach (T resource in pluginManagerResources)
            {
                resource.SetLogger(Logger.Create(resource.GetType()));
            }
        }
    }
}
