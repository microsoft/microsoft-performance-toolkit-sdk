﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

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
        private ILogger logger;

        /// <param name="stream">
        ///     Stream for reading the plugin package file.
        /// </param>
        public static bool TryCreate(
            Stream stream,
            bool leaveOpen,
            out PluginPackage package)
        {
            return TryCreate(stream, leaveOpen, Logger.Create<PluginPackage>(), out package);
        }

        public static bool TryCreate(
            Stream stream,
            out PluginPackage package)
        {
            return TryCreate(stream, false, out package);
        }

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
            catch
            {
                package = null;
                return false;
            }
        }

        /// <param name="fileName">
        ///     The full file name of the plugin package.
        /// </param>
        public static bool TryCreate(
            string fileName,
            out PluginPackage package)
        {
            return TryCreate(fileName, Logger.Create<PluginPackage>(), out package);
        }

        public static bool TryCreate(
            string fileName,
            ILogger logger,
            out PluginPackage package)
        {
            try
            {
                using (Stream stream = OpenFile(fileName))
                {
                    return TryCreate(stream, out package);
                }
            }
            catch (Exception e)
            {
                package = null;
                logger.Error(e, $"Failed to read plugin package from file {fileName}");

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
        /// 
        /// </param>
        private PluginPackage(
            Stream stream, 
            bool leaveOpen,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            this.logger = Logger.Create<PluginPackage>();

            try
            {
                this.zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
                this.Entries = this.zip.Entries.Select(e => new PluginPackageEntry(e)).ToList().AsReadOnly();
                this.PluginMetadata = ReadMetadata();
            }
            catch when (!leaveOpen)
            {
                stream.Dispose();
                throw;
            }
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
        ///     An await-able task whose result represents whether the extraction succeeds.
        /// </returns>
        public Task<bool> ExtractPackageAsync(
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
        ///     An await-able task whose result represents whether the extraction succeeds.
        /// </returns>
        public Task<bool> ExtractEntriesAsync(
            string extractPath,
            Func<PluginPackageEntry, bool> predicate,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return ExtractEntriesInternalAsync(
                this.Entries.Where(e => predicate(e)),
                extractPath,
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

        private static async Task<bool> ExtractEntriesInternalAsync(
           IEnumerable<PluginPackageEntry> entries,
           string extractPath,
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

            // TODO: #238 Error handling
            // TODO: #257 Report progress
            foreach (PluginPackageEntry entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destPath = Path.GetFullPath(Path.Combine(extractPath, entry.RelativePath));
                if (entry.IsDirectory )
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

            return true;
        }

        private static Stream OpenFile(string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private PluginMetadata ReadMetadata()
        {
            ZipArchiveEntry entry = this.zip.GetEntry(pluginMetadataFileName);
            if (entry == null)
            {
                throw new InvalidDataException($"{pluginMetadataFileName} not found in package.");
            }

            using (Stream stream = entry.Open())
            {
                PluginMetadata pluginMetadata = SerializationUtils.ReadFromStream<PluginMetadata>(
                    stream,
                    SerializationConfig.PluginsManagerSerializerDefaultOptions,
                    this.logger);

                if (pluginMetadata == null)
                {
                    // TODO:
                    throw new InvalidDataException("");
                    // InvalidDataExceptionFactory.CreateInvalidPluginMetadataException();
                }

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
