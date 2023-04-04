// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
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
        private readonly ISerializer<PluginMetadata> metadataSerializer;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZipPluginPackageReader"/>
        /// </summary>
        /// <param name="metadataSerializer">
        ///     The serializer to use to deserialize the plugin metadata.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory to use to create loggers.
        /// </param>
        public ZipPluginPackageReader(
            ISerializer<PluginMetadata> metadataSerializer,
            Func<Type, ILogger> loggerFactory)
        {
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
                ZipArchiveEntry contentFolderEntry = zip.GetEntry(PackageConstants.PluginContentPath);
                if (contentFolderEntry == null)
                {
                    this.logger.Error($"Plugin content folder {PackageConstants.PluginContentPath} not found in the plugin package.");
                    return null;
                }

                // Check that the plugin metadata file exists
                ZipArchiveEntry metadataEntry = zip.GetEntry(PackageConstants.PluginMetadataFileName);
                if (metadataEntry == null)
                {
                    this.logger.Error($"Plugin metadata file {PackageConstants.PluginMetadataFileName} not found in the plugin package.");
                    return null;
                }

                // Try to read plugin metadata
                PluginMetadata metadata;
                try
                {
                    using (Stream metadataStream = metadataEntry.Open())
                    {
                        metadata = await this.metadataSerializer.DeserializeAsync(stream, cancellationToken);
                    }
                }
                catch (JsonException e)
                {
                    this.logger.Error(e, $"Deserialization failed due to invalid JSON text in plugin metadata");
                    return null;
                }

                success = true;
                return new ZipPluginPackage(metadata, zip, this.loggerFactory);
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
