// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public sealed class PluginsManagerResourceRepository<T>
        : IPluginsManagerResourceLoader<T>,
          IRepositoryRO<T>
        where T : IPluginsManagerResource
    {
        private readonly HashSet<PluginsManagerResourceReference> resources;
        private readonly object mutex = new object();

        /// <summary>
        ///     Initializes a <see cref="PluginsManagerResourceRepository{T}"/> with an empty collection of resources.
        /// </summary>
        public PluginsManagerResourceRepository()
            : this(Enumerable.Empty<T>())
        {
        }

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
        public IEnumerable<T> Items
        {
            get
            {
                lock (this.mutex)
                {
                    return this.resources.Select(r => r.Instance).OfType<T>();
                }
            }
        }

        /// <summary>
        ///     Adds a new resource to the repository.
        /// </summary>
        /// <param name="item">
        ///     The resource to add.
        /// </param>
        public void Add(T item)
        {
            Guard.NotNull(item, nameof(item));

            lock (this.mutex)
            {
                var resourceReference = new PluginsManagerResourceReference(item);

                if (this.resources.Add(resourceReference))
                {
                    NewResourcesAddedCommon(new[] { resourceReference });
                }
            }
        }

        /// <summary>
        ///     Adds a collection of resources to the repository.
        /// </summary>
        /// <param name="items">
        ///     The resources to add.
        /// </param>
        public void Add(IEnumerable<T> items)
        {
            Guard.NotNull(items, nameof(items));

            lock (this.mutex)
            {
                IEnumerable<PluginsManagerResourceReference> newResources = items
                    .Select(r => new PluginsManagerResourceReference(r))
                    .Where(r => this.resources.Add(r));

                NewResourcesAddedCommon(newResources);
            }
        }

        /// <inheritdoc />
        public void OnNewResourcesLoaded(IEnumerable<PluginsManagerResourceReference> loadedResources)
        {
            Guard.NotNull(loadedResources, nameof(loadedResources));

            lock (this.mutex)
            {
                IEnumerable<PluginsManagerResourceReference> newResources = loadedResources
                    .Where(r => r.Instance is T)
                    .Where(r => this.resources.Add(r));

                NewResourcesAddedCommon(newResources);
            }
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void NewResourcesAddedCommon(IEnumerable<PluginsManagerResourceReference> newResources)
        {
            if (newResources.Any())
            {
                SetResourcesLoggers(newResources.Select(r => r.Instance).OfType<T>());
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    newResources));
            };
        }

        private void SetResourcesLoggers(IEnumerable<T> pluginsManagerResources)
        {
            foreach (T resource in pluginsManagerResources)
            {
                resource.SetLogger(Logger.Create(resource.GetType()));
            }
        }
    }
}
