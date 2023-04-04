﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    public sealed class ZipPluginPackage
        : PluginPackage
    {
        private bool disposedValue;
        private readonly ZipArchive zip;
        private readonly Lazy<IReadOnlyCollection<PluginPackageEntry>> zipEntries;

        /// <summary>
        ///     Creates an instance of <see cref="ZipPluginPackage"/>.
        /// </summary>
        /// <param name="metadata">
        ///     The metadata of the plugin.
        /// </param>
        /// <param name="zip">
        ///     The zip archive that contains the plugin.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        public ZipPluginPackage(
            PluginMetadata metadata,
            ZipArchive zip,
            Func<Type, ILogger> loggerFactory)
            : base(metadata, loggerFactory)
        {
            Guard.NotNull(zip, nameof(zip));
            
            this.zip = zip;
            this.zipEntries = new Lazy<IReadOnlyCollection<PluginPackageEntry>>(
                () => this.zip.Entries.Select(e => new ZipPluginPackageEntry(e)).ToList());
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<PluginPackageEntry> Entries
        {
            get
            {
                return this.zipEntries.Value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.zip.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
