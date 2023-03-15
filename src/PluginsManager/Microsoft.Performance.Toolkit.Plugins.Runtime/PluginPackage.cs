// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    public sealed class PluginPackage
        : IDisposable
    {
        private readonly ZipArchive zip;
        private static readonly string pluginContentPath = "plugin/";
        private static readonly string pluginMetadataFileName = "pluginspec.json";

        private bool disposedValue;
        private readonly ILogger logger;

        /// <summary>
        ///     Tries to create an instance of <see cref="PluginPackage"/> with the specified parameters.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
        /// </param>
        /// <param name="metadataReader">
        ///     Used to deserialize the plugin metadata.
        /// </param>
        /// <param name="leaveOpen">
        ///     <c>true</c> to leave <paramref name = "stream" /> open after <see cref="PluginPackage"/> is disposed.
        /// </param>
        /// <param name="package">
        ///     The created <see cref="PluginPackage"/> instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="PluginPackage"/> was created successfully. <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Stream stream,
            IDataReader<Stream, PluginMetadata> metadataReader,
            bool leaveOpen,
            out PluginPackage package)
        {
            return TryCreate(stream, metadataReader, leaveOpen, Logger.Create<PluginPackage>(), out package);
        }

        /// <summary>
        ///     Creates an instance of <see cref="PluginPackage"/> with the specified parameters.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
        /// </param>
        /// <param name="metadataReader">
        ///     Used to deserialize the plugin metadata.
        /// </param>
        /// <param name="leaveOpen">
        ///     <c>true</c> to leave <paramref name = "stream" /> open after <see cref="PluginPackage"/> is disposed.
        ///     <c>false</c> otherwise.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        /// <param name="package">
        ///     The created <see cref="PluginPackage"/> instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="PluginPackage"/> was created successfully. <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Stream stream,
            IDataReader<Stream, PluginMetadata> metadataReader,
            bool leaveOpen,     
            ILogger logger,
            out PluginPackage package)
        {
            try
            {
                package = new PluginPackage(stream, metadataReader, leaveOpen, logger);
                return true;
            }
            catch (Exception e)
            {
                if (!leaveOpen)
                {
                    stream.Dispose();
                }

                if (e is MalformedPluginPackageException)
                {
                    logger.Error(e, e.Message);
                }
                else if (e is JsonException)
                {
                    logger.Error(e, $"Deserialization failed due to invalid JSON text in plugin metadata");
                }
                else if (e is InvalidDataException)
                {
                    logger.Error(e, $"Invalid stream for a plugin package.");
                }

                package = null;
                return false;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="PluginPackage"/>.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
        /// </param>
        /// <param name="metadataReader">
        ///     Used to read the plugin metadata.
        /// </param>
        /// <param name="leaveOpen">
        ///     <c>true</c> to leave <paramref name = "stream" /> open after <see cref="PluginPackage"/> is disposed.
        ///     <c>false</c> otherwise.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        private PluginPackage(
            Stream stream,
            IDataReader<Stream, PluginMetadata> metadataReader,
            bool leaveOpen,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            this.logger = logger;
            this.zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
            this.Entries = this.zip.Entries.Select(e => new PluginPackageEntry(e)).ToList().AsReadOnly();
            this.PluginMetadata = ReadMetadata(metadataReader);
        }
        
        /// <summary>
        ///     Gets the relative file path to plugin content.
        /// </summary>
        public static string PluginContentPath
        {
            get
            {
                return pluginContentPath;
            }
        }

        /// <summary>
        ///     Gets the name of the metadata file.
        /// </summary>
        public static string PluginMetadataFileName
        {
            get
            {
                return pluginMetadataFileName;
            }
        }

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

        /// <summary>
        ///     Gets all entries (files and directories) contained in this plugin package.
        /// </summary>
        public IReadOnlyCollection<PluginPackageEntry> Entries { get; }

        /// <summary>
        ///     Gets the plugin metadata object.
        /// </summary>
        public PluginMetadata PluginMetadata { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Id} - {this.Version}";
        }

        private PluginMetadata ReadMetadata(IDataReader<Stream, PluginMetadata> metadataReader)
        {
            ZipArchiveEntry entry = this.zip.GetEntry(pluginMetadataFileName);
            if (entry == null)
            {
                throw new MalformedPluginPackageException($"{pluginMetadataFileName} not found in the plugin package.");
            }

            using (Stream stream = entry.Open())
            {
                PluginMetadata pluginMetadata = metadataReader.ReadData(stream);
                return pluginMetadata;
            }
        }

        private void Dispose(bool disposing)
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
