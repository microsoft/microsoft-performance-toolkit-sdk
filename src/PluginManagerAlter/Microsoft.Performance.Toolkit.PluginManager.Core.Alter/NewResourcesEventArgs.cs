using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter
{
    public class NewResourcesEventArgs<T> : EventArgs where T :IPluginResource
    {
        public NewResourcesEventArgs(IEnumerable<T> newPluginResources)
        {
            this.NewPluginResources = newPluginResources;
        }
            
        public IEnumerable<T> NewPluginResources { get; }
    }
}
