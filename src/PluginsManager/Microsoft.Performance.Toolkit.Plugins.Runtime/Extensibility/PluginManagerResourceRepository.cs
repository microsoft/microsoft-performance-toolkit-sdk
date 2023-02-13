// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public class PluginManagerResourceRepository<T>
        : IPluginManagerResourceRepository<T>
        where T : class, IPluginManagerResource
    {
        private readonly HashSet<T> resources;

        /// <summary>
        ///     Initializes a <see cref="PluginManagerResourceRepository{T}"/> with an existing collection of <paramref name="resources"/>.
        /// </summary>
        /// <param name="resources">
        ///     A collection of <see cref="IPluginManagerResource"s of type <see cref="T"/>.
        /// </param>
        public PluginManagerResourceRepository(IEnumerable<T> resources)
        {
            this.resources = new HashSet<T>(resources, PluginManagerResourceComparer.Instance);
        }

        /// <inheritdoc />
        public IEnumerable<T> PluginResources
        {
            get
            {
                return this.resources;
            }
        }

        /// <inheritdoc />
        public void OnNewResourcesLoaded(IEnumerable<T> loadedResources)
        {
            lock (this.resources)
            {
                IEnumerable<T> newResources = loadedResources.Except(this.resources);
                this.resources.UnionWith(newResources);

                if (newResources.Any())
                {
                    ResourcesAdded?.Invoke(this, new NewResourcesEventArgs<T>(newResources));
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<NewResourcesEventArgs<T>> ResourcesAdded;

        private sealed class PluginManagerResourceComparer
            : IEqualityComparer<T>
        {
            public static readonly IEqualityComparer<T> Instance = new PluginManagerResourceComparer();

            public bool Equals(T x, T y)
            {
                Debug.Assert(x != null);
                Debug.Assert(y != null);

                Guid xGuid = x.TryGetGuid();
                Guid yGuid = y.TryGetGuid();

                Debug.Assert(xGuid != Guid.Empty);
                Debug.Assert(yGuid != Guid.Empty);

                return xGuid.Equals(yGuid);
            }

            public int GetHashCode(T obj)
            {
                Debug.Assert(obj != null);

                Guid guid = obj.TryGetGuid();

                Debug.Assert(guid != Guid.Empty);

                return guid.GetHashCode();
            }
        }
    }
}
