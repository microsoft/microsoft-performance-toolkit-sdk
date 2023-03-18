// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///    Represents a repository of <see cref="PluginSource"/>s.
    /// </summary>
    public class PluginSourceRepository
        : IRepository<PluginSource>
    {
        private HashSet<PluginSource> currentSources;
        private readonly object mutex = new object();

        /// <summary>
        ///    Creates an instance of the <see cref="PluginSourceRepository"/>.
        /// </summary>
        public PluginSourceRepository()
        {
            this.currentSources = new HashSet<PluginSource>();
        }

        public IEnumerable<PluginSource> Items
        {
            get
            {
                lock (this.mutex)
                {
                    return this.currentSources;
                }
            }
            private set
            {
                if (this.currentSources != value)
                {
                    this.currentSources = new HashSet<PluginSource>(value);

                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        /// <summary>
        ///    Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     Adds the given <see cref="PluginSource"/>s to the repository.
        /// </summary>
        /// <param name="sources"></param>
        public void AddPluginSourcesAsync(IEnumerable<PluginSource> sources)
        {
            this.Items = sources;
        }

        /// <summary>
        ///    Clears the repository of all <see cref="PluginSource"/>s.
        /// </summary>
        public void ClearPluginSources()
        {
            this.Items = new List<PluginSource>();
        }
    }
}
