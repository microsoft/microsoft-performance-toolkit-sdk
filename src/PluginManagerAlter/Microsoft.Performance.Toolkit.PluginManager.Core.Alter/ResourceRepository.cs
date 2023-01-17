using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter
{
    public class ResourceRepository<T> where T : IPluginResource
    {
        private readonly HashSet<T> resources;

        public  ResourceRepository(IEnumerable<T> resources)
        {
            this.resources = new HashSet<T>(resources);
        }

        public IEnumerable<T> PluginResources { get { return this.resources; } }

        public void OnResourcesLoaded(IEnumerable<T> loadedResources)
        {
            IEnumerable<T> newResources = loadedResources.Except(this.resources);
            this.resources.UnionWith(newResources);

            if (newResources.Any())
            {
                ResourcesChanged?.Invoke(this, new NewResourcesEventArgs<T>(newResources));
            }
        }

        public event EventHandler<NewResourcesEventArgs<T>> ResourcesChanged;
    }
}
