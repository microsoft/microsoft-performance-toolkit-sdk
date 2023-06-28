// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    /// <summary>
    ///     Reads a plugin package from a zip archive.
    /// </summary>
    public sealed class ZipPluginPackageReader
        : IPluginPackageReader
    {
        private readonly ISerializer<PluginInfo> infoSerializer;
        private readonly ISerializer<PluginContents> metadataSerializer;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZipPluginPackageReader"/>
        /// </summary>
        /// <param name="infoSerializer">
        ///     The serializer to use to deserialize the plugin info.
        /// </param>
        /// <param name="metadataSerializer">
        ///     The serializer to use to deserialize the plugin contents.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory to use to create loggers.
        /// </param>
        public ZipPluginPackageReader(
            ISerializer<PluginInfo> infoSerializer,
            ISerializer<PluginContents> metadataSerializer,
            Func<Type, ILogger> loggerFactory)
        {
            this.infoSerializer = infoSerializer;
            this.metadataSerializer = metadataSerializer;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(ZipPluginPackageReader));
        }

        /// <inheritdoc/>
        public async Task<PluginPackage> TryReadPackageAsync(
            Stream stream,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(stream, nameof(stream));

            bool success = false;

            // Try to open the stream as a zip archive
            ZipArchive zip;
            try
            {
                zip = new ZipArchive(stream, ZipArchiveMode.Read, false);
            }
            catch (ArgumentException e)
            {
                this.logger.Error(e, $"Unable to read from the stream.");
                return null;
            }
            catch (InvalidDataException e)
            {
                this.logger.Error(e, $"The stream could not be interpreted as a zip archive.");
                return null;
            }

            try
            {
                // Check that the plugin content folder exists
                bool hasContentFolder = zip.Entries.Any(
                    e => e.FullName.Replace('\\', '/').StartsWith(PackageConstants.PluginContentFolderName, StringComparison.OrdinalIgnoreCase));

                if (!hasContentFolder)
                {
                    this.logger.Error($"Plugin content folder {PackageConstants.PluginContentFolderName} not found in the plugin package.");
                    return null;
                }

                // Check that the plugin info file exists
                ZipArchiveEntry infoFileEntry = zip.GetEntry(PackageConstants.PluginInfoFileName);
                if (infoFileEntry == null)
                {
                    this.logger.Error($"Plugin info file {PackageConstants.PluginInfoFileName} not found in the plugin package.");
                    return null;
                }

                // Try to read plugin info
                PluginInfo info;
                try
                {
                    using (Stream infoStream = infoFileEntry.Open())
                    {
                        info = await this.infoSerializer.DeserializeAsync(infoStream, cancellationToken);
                    }
                }
                catch (JsonException e)
                {
                    this.logger.Error(e, $"Deserialization failed due to invalid JSON text in plugin info file");
                    return null;
                }

                // Check that the plugin metadata file exists
                ZipArchiveEntry contentsFileEntry = zip.GetEntry(PackageConstants.PluginContentsFileName);
                if (contentsFileEntry == null)
                {
                    this.logger.Error($"Plugin contents file {PackageConstants.PluginContentsFileName} not found in the plugin package.");
                    return null;
                }

                // Try to read plugin metadata
                PluginContents contents;
                try
                {
                    using (Stream contentsFileStream = contentsFileEntry.Open())
                    {
                        contents = await this.metadataSerializer.DeserializeAsync(contentsFileStream, cancellationToken);
                    }
                }
                catch (JsonException e)
                {
                    this.logger.Error(e, $"Deserialization failed due to invalid JSON text in plugin contents file");
                    return null;
                }

                success = true;
                return new ZipPluginPackage(info, contents, zip, this.loggerFactory);
            }
            finally
            {
                if (!success)
                {
                    zip.Dispose();
                }
            }
        }
    }
}
