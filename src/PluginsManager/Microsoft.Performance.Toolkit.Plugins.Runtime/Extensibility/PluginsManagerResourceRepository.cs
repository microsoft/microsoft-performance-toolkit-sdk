// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public class PluginsManagerResourceRepository<T>
        : IPluginsManagerResourcesConsumer,
          IPluginsManagerResourceRepository<T>
        where T : IPluginsManagerResource
    {
        private readonly HashSet<PluginsManagerResourceReference> resources;

        /// <summary>
        ///     Initializes a <see cref="PluginsManagerResourceRepository{T}"/> with an existing collection of <paramref name="resources"/>.
        /// </summary>
        /// <param name="resources">
        ///     A collection of <see cref="IPluginsManagerResource"s of type <see cref="T"/>.
        /// </param>
        public PluginsManagerResourceRepository(IEnumerable<T> resources)
        {
            Guard.NotNull(resources, nameof(resources));

            SetResourcesLoggers(resources);
            this.resources = new HashSet<PluginsManagerResourceReference>(resources.Select(r => new PluginsManagerResourceReference(r)));
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
        public void OnNewResourcesLoaded(IEnumerable<PluginsManagerResourceReference> loadedResources)
        {
            Guard.NotNull(loadedResources, nameof(loadedResources));

            lock (this.resources)
            {
                IEnumerable<PluginsManagerResourceReference> newResources = loadedResources
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
        
        private void SetResourcesLoggers(IEnumerable<T> pluginsManagerResources)
        {
            foreach (T resource in pluginsManagerResources)
            {
                resource.SetLogger(Logger.Create(resource.GetType()));
            }
        }
    }
}
