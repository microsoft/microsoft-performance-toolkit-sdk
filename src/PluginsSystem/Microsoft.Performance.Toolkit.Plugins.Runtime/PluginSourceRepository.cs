// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///    Represents a repository of <see cref="PluginSource"/>s.
    /// </summary>
    public sealed class PluginSourceRepository
        : IRepository<PluginSource>
    {
        private readonly HashSet<PluginSource> currentSources;
        private readonly ILogger logger;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly object mutex = new object();

        /// <summary>
        ///    Creates an instance of the <see cref="PluginSourceRepository"/>.
        /// </summary>
        public PluginSourceRepository()
            : this(Logger.Create)
        {
        }

        /// <summary>
        ///     Creates an instance of the <see cref="PluginSourceRepository"/> with a logger factory.
        /// </summary>
        /// <param name="loggerFactory">
        ///     A factory that creates loggers for the given type. If <c>null</c>, a default logger will be used.
        /// </param>
        public PluginSourceRepository(Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.currentSources = new HashSet<PluginSource>();
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(PluginSourceRepository));
        }

        /// <inheritdoc/>
        public IEnumerable<PluginSource> Items
        {
            get
            {
                lock (this.mutex)
                {
                    return this.currentSources;
                }
            }
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc/>
        public void Add(IEnumerable<PluginSource> pluginSources)
        {
            lock (this.mutex)
            {
                IEnumerable<PluginSource> addedItems = pluginSources.Where(x => this.currentSources.Add(x));

                if (addedItems.Any())
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems.ToList()));
                }
            }
        }

        /// <inheritdoc />
        public void Add(PluginSource pluginSource)
        {
            lock (this.mutex)
            {
                bool success = this.currentSources.Add(pluginSource);
                if (success)
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pluginSource));
                }
            }
        }

        /// <inheritdoc/>
        public void Remove(PluginSource pluginSource)
        {
            lock (this.mutex)
            {
                bool success = this.currentSources.Remove(pluginSource);
                if (success)
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, pluginSource));
                }
            }
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<PluginSource> pluginSources)
        {
            lock (this.mutex)
            {
                IEnumerable<PluginSource> removedItems = pluginSources.Where(x => this.currentSources.Remove(x));
                if (removedItems.Any())
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems.ToList()));
                }
            }
        }
    }
}
