using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    public class PluginSourceRepo
        : IReadonlyRepository<PluginSource>
    {
        private HashSet<PluginSource> currentSources;

        public IEnumerable<PluginSource> Items
        {
            get
            {
                return this.currentSources;
            }
            private set
            {
                if (this.currentSources != value)
                {
                    ItemsModified?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ItemsModified;

        public void AddPluginSourcesAsync(IEnumerable<PluginSource> sources)
        {
            this.Items = this.currentSources.Union(sources);
        }

        public void ClearPluginSources()
        {
            this.Items = new List<PluginSource>();
        }
    }
}
