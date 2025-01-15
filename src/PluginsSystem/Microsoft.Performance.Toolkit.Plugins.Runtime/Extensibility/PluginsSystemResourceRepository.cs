// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public sealed class PluginsSystemResourceRepository<T>
        : IPluginsSystemResourceLoader<T>,
          IRepositoryRO<T>
        where T : IPluginsSystemResource
    {
        private readonly HashSet<PluginsSystemResourceReference> resources;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly object mutex = new object();

        /// <summary>
        ///     Initializes a <see cref="PluginsSystemResourceRepository{T}"/> with an empty collection of resources and a logger factory.
        /// </summary>
        /// <param name="loggerFactory">
        ///     A factory that creates loggers for the given type.
        /// </param>
        public PluginsSystemResourceRepository(Func<Type, ILogger> loggerFactory)
            : this(Enumerable.Empty<T>(), loggerFactory)
        {
        }

        /// <summary>
        ///     Initializes a <see cref="PluginsSystemResourceRepository{T}"/> with an existing collection of <paramref name="resources"/>
        ///     and a logger factory.
        /// </summary>
        /// <param name="resources">
        ///     A collection of <see cref="IPluginsSystemResource"s of type <see cref="T"/>.
        /// </param>
        /// <param name="loggerFactory">
        ///     A factory that creates loggers for the given type.
        /// </param>
        public PluginsSystemResourceRepository(
            IEnumerable<T> resources,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(resources, nameof(resources));

            this.loggerFactory = loggerFactory;
            SetResourcesLoggers(resources.ToArray());
            this.resources = new HashSet<PluginsSystemResourceReference>(resources.Select(r => new PluginsSystemResourceReference(r)));
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
                var resourceReference = new PluginsSystemResourceReference(item);

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
                PluginsSystemResourceReference[] newResources = items
                    .Select(r => new PluginsSystemResourceReference(r))
                    .Where(r => this.resources.Add(r))
                    .ToArray();

                NewResourcesAddedCommon(newResources);
            }
        }

        /// <inheritdoc />
        public void OnNewResourcesLoaded(IEnumerable<PluginsSystemResourceReference> loadedResources)
        {
            Guard.NotNull(loadedResources, nameof(loadedResources));

            lock (this.mutex)
            {
                PluginsSystemResourceReference[] newResources = loadedResources
                    .Where(r => r.Instance is T)
                    .Where(r => this.resources.Add(r))
                    .ToArray();

                NewResourcesAddedCommon(newResources);
            }
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void NewResourcesAddedCommon(PluginsSystemResourceReference[] newResources)
        {
            if (newResources.Any())
            {
                SetResourcesLoggers(newResources.Select(r => r.Instance).OfType<T>().ToArray());
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    newResources));
            };
        }

        private void SetResourcesLoggers(T[] pluginsSystemResources)
        {
            foreach (T resource in pluginsSystemResources)
            {
                resource.SetLogger(this.loggerFactory(resource.GetType()));
            }
        }
    }
}
