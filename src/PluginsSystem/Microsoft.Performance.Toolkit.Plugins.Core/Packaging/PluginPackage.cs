// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

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
        /// <param name="metadata">
        ///     The plugin metadata.
        /// </param>
        /// <param name="contentsMetadata">
        ///     The plugin contents metadata.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory.
        /// </param>
        protected PluginPackage(
            PluginMetadata metadata,
            PluginContentsMetadata contentsMetadata,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(metadata, nameof(metadata));
            Guard.NotNull(contentsMetadata, nameof(contentsMetadata));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.Metadata = metadata;
            this.ContentsMetadata = contentsMetadata;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(GetType());
        }

        /// <summary>
        ///     Gets the entries in the plugin package.
        /// </summary>
        public abstract IReadOnlyCollection<PluginPackageEntry> Entries { get; }

        /// <summary>
        ///     Gets the metadata of this plugin.
        /// </summary>
        public PluginMetadata Metadata { get; }

        /// <summary>
        ///     Gets the metadata of the plugin contents.
        /// </summary>
        public PluginContentsMetadata ContentsMetadata { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Metadata.Identity.ToString();
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
