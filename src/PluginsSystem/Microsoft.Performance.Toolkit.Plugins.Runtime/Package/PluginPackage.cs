// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    public abstract class PluginPackage
        : IDisposable
    {
        private readonly Func<Type, ILogger> loggerFactory;
        protected readonly ILogger logger;

        protected PluginPackage(
            PluginMetadata pluginMetadata,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginMetadata, nameof(pluginMetadata));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.PluginMetadata = pluginMetadata;       
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(GetType());
        }

        public abstract IReadOnlyCollection<PluginPackageEntry> Entries { get; }

        /// <summary>
        ///    Gets the plugin metadata.
        /// </summary>
        public PluginMetadata PluginMetadata { get; }

        public PluginIdentity PluginIdentity { get; }

        /// <summary>
        ///     Gets the plugin ID.
        /// </summary>
        public string Id
        {
            get
            {
                return this.PluginMetadata.Id;
            }
        }

        /// <summary>
        ///     Gets the plugin version.
        /// </summary>
        public Version Version
        {
            get
            {
                return this.PluginMetadata.Version;
            }
        }

        /// <summary>
        ///     Gets the plugin display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.PluginMetadata.DisplayName;
            }
        }

        /// <summary>
        ///     Gets the plugin description.
        /// </summary>
        public string Description
        {
            get
            {
                return this.PluginMetadata.Description;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Id} - {this.Version}";
        }

        public abstract Task ExtractPackageAsync(
            string extractPath,
            CancellationToken cancellationToken,
            IProgress<int> progress);

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
