// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///    Represents a repository of <see cref="PluginSource"/>s. Only distinct sources are added.
    /// </summary>
    public sealed class PluginSourceRepository
        : ThreadSafeRepository<PluginSource>
    {
        private readonly HashSet<PluginSource> currentSources;
        private readonly ILogger logger;
        private readonly Func<Type, ILogger> loggerFactory;

        /// <summary>
        ///     Creates an instance of the <see cref="PluginSourceRepository"/>.
        /// </summary>
        /// <param name="loggerFactory">
        ///     Used to create loggers.
        /// </param>
        public PluginSourceRepository(Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(PluginSourceRepository));
            this.currentSources = new HashSet<PluginSource>();
        }

        /// <inheritdoc/>
        protected override bool AddInternal(PluginSource source)
        {
            return this.currentSources.Add(source);
        }

        /// <inheritdoc/>
        protected override IEnumerable<PluginSource> AddInternal(IEnumerable<PluginSource> sources)
        {
            return sources.Where(x => this.currentSources.Add(x)).ToList();
        }

        /// <inheritdoc/>
        protected override bool RemoveInternal(PluginSource source)
        {
            return this.currentSources.Remove(source);
        }

        /// <inheritdoc/>
        protected override IEnumerable<PluginSource> RemoveInternal(IEnumerable<PluginSource> sources)
        {
            return sources.Where(x => this.currentSources.Remove(x)).ToList();
        }

        /// <inheritdoc/>
        protected override IEnumerable<PluginSource> GetItemsInternal()
        {
            return this.currentSources;
        }
    }
}
