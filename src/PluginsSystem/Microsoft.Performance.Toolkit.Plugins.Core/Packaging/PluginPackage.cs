// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    public abstract class PluginPackage
        : IDisposable
    {
        protected readonly Func<Type, ILogger> loggerFactory;
        protected readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of the <see cref="PluginPackage"/>.
        /// </summary>
        /// <param name="pluginInfo">
        ///     The plugin info.
        /// </param>
        /// <param name="pluginContents">
        ///     The plugin contents.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory.
        /// </param>
        protected PluginPackage(
            PluginInfo pluginInfo,
            PluginContents pluginContents,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginInfo, nameof(pluginInfo));
            Guard.NotNull(pluginContents, nameof(pluginContents));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.PluginInfo = pluginInfo;
            this.PluginContents = pluginContents;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(GetType());
        }

        /// <summary>
        ///     Gets the entries in the plugin package.
        /// </summary>
        public abstract IReadOnlyCollection<PluginPackageEntry> Entries { get; }

        /// <summary>
        ///     Gets the plugin information.
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        ///     Gets the plugin contents.
        /// </summary>
        public PluginContents PluginContents { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.PluginInfo.Identity.ToString();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
