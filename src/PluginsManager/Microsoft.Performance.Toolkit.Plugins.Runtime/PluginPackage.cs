// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
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
        /// <param name="package">
        ///     The created <see cref="PluginPackage"/> instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="PluginPackage"/> was created successfully. <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Stream stream,
            out PluginPackage package)
        {
            return TryCreate(stream, false, out package);
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="PluginPackage"/> with the specified parameters.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
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
            bool leaveOpen,
            out PluginPackage package)
        {
            return TryCreate(stream, leaveOpen, Logger.Create<PluginPackage>(), out package);
        }

        /// <summary>
        ///     Creates an instance of <see cref="PluginPackage"/> with the specified parameters.
        /// </summary>
        /// <param name="stream">
        ///     Stream for reading the plugin package file.
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
            bool leaveOpen,
            ILogger logger,
            out PluginPackage package)
        {
            try
            {
                package = new PluginPackage(stream, leaveOpen, logger);
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
        /// <param name="leaveOpen">
        ///     <c>true</c> to leave <paramref name = "stream" /> open after <see cref="PluginPackage"/> is disposed.
        ///     <c>false</c> otherwise.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        private PluginPackage(
            Stream stream,
            bool leaveOpen,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            this.logger = logger;
            this.zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
            this.Entries = this.zip.Entries.Select(e => new PluginPackageEntry(e)).ToList().AsReadOnly();
            this.PluginMetadata = ReadMetadata();
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

        /// <summary>
        ///     Extracts all files in this package.
        /// </summary>
        /// <param name="extractPath">
        ///     The path to which the files will be extracted.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the extraction.
        /// </param>
        /// <returns>
        ///     An await-able task.
        /// </returns>
        public Task ExtractPackageAsync(
            string extractPath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return ExtractEntriesAsync(extractPath, entry => true, cancellationToken, progress);
        }

        /// <summary>
        ///     Extract certain entires from the package.
        /// </summary>
        /// <param name="extractPath">
        ///     The path to which the files will be extracted.
        /// </param>
        /// <param name="predicate">
        ///     A function whose result indicates whether an entry should be extracted.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the extraction.
        /// </param>
        /// <returns>
        ///     An await-able task.
        /// </returns>
        public Task ExtractEntriesAsync(
            string extractPath,
            Func<PluginPackageEntry, bool> predicate,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return ExtractEntriesInternalAsync(
                this.Entries.Where(e => predicate(e)),
                extractPath,
                this.logger,
                cancellationToken,
                progress);
        }

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

        private static async Task ExtractEntriesInternalAsync(
           IEnumerable<PluginPackageEntry> entries,
           string extractPath,
           ILogger logger,
           CancellationToken cancellationToken,
           IProgress<int> progress)
        {
            Guard.NotNull(extractPath, nameof(extractPath));

            const int bufferSize = 4096;
            const int defaultAsyncBufferSize = 81920;

            extractPath = Path.GetFullPath(extractPath);
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                extractPath += Path.DirectorySeparatorChar;
            }

            // TODO: #257 Report progress
            try
            {
                foreach (PluginPackageEntry entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string destPath = Path.GetFullPath(Path.Combine(extractPath, entry.RelativePath));
                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    else
                    {
                        string destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }

                        using (Stream entryStream = entry.Open())
                        using (var destStream = new FileStream(
                            destPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None,
                            bufferSize,
                            FileOptions.Asynchronous | FileOptions.SequentialScan))
                        {
                            await entryStream.CopyToAsync(destStream, defaultAsyncBufferSize, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                string errorMsg = $"Unable to extract plugin content to {extractPath}";
                logger.Error(e, errorMsg);
                throw new PluginPackageExtractionException(errorMsg, e);
            }
        }

        private PluginMetadata ReadMetadata()
        {
            ZipArchiveEntry entry = this.zip.GetEntry(pluginMetadataFileName);
            if (entry == null)
            {
                throw new MalformedPluginPackageException($"{pluginMetadataFileName} not found in the plugin package.");
            }

            using (Stream stream = entry.Open())
            {
                PluginMetadata pluginMetadata = JsonSerializer.Deserialize<PluginMetadata>(
                    stream,
                    SerializationConfig.PluginsManagerSerializerDefaultOptions);

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
