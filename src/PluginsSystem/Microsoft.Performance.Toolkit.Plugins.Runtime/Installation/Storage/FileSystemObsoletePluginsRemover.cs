﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Removes obsolete plugins from the file system.
    /// </summary>
    public sealed class FileSystemObsoletePluginsRemover
        : IObsoletePluginsRemover
    {
        private readonly IPluginRegistry pluginRegistry;
        private readonly IPluginsStorageDirectory pluginsStorageDirectory;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystemObsoletePluginsRemover"/>
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The plugin registry to use to determine which plugins are installed.
        /// </param>
        /// <param name="pluginsStorageDirectory">
        ///     The plugins storage directory to look for obsolete plugins in.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory to use to create loggers.
        /// </param>
        public FileSystemObsoletePluginsRemover(
            IPluginRegistry pluginRegistry,
            IPluginsStorageDirectory pluginsStorageDirectory,
            Func<Type, ILogger> loggerFactory)
        {
            this.pluginRegistry = pluginRegistry;
            this.pluginsStorageDirectory = pluginsStorageDirectory;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(FileSystemObsoletePluginsRemover));
        }

        /// <inheritdoc/>
        public async Task ClearObsoleteAsync(CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllAsync(cancellationToken);
                IEnumerable<PluginIdentity> installedPluginIds = installedPlugins.Select(p => p.Metadata.Identity);

                IEnumerable<string> dirsInUse = installedPluginIds.Select(
                   p => this.pluginsStorageDirectory.GetRootDirectory(p));

                IEnumerable<string> dirsToRemove = this.pluginsStorageDirectory.GetAllRootDirectories()
                    .Where(d => !dirsInUse.Contains(d, StringComparer.OrdinalIgnoreCase));

                List<(string dirToRemove, Task<bool> task)> dirTaskTuples = dirsToRemove
                    .Select(dirToRemove => (dirToRemove: dirToRemove, task: TryDeleteDirectory(dirToRemove, cancellationToken)))
                    .ToList();

                try
                {
                    await Task.WhenAll(dirTaskTuples.Select(t => t.task)).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    this.logger.Info($"The request to clean up plugins from the storage is cancelled.");

                    dirTaskTuples
                        .Where(p => p.task.Status == TaskStatus.RanToCompletion && p.task.Result)
                        .ToList()
                        .ForEach(t => this.logger.Info($"{t.dirToRemove} has been successfully deleted"));

                    throw;
                }
                catch
                {
                }

                foreach (var tuple in dirTaskTuples)
                {
                    if (tuple.task.IsFaulted || tuple.task.IsCanceled || !tuple.task.Result)
                    {
                        this.logger.Error(tuple.task.Exception, $"Failed to clean up directory {tuple.dirToRemove}");
                    }
                    else
                    {
                        this.logger.Info($"{tuple.dirToRemove} has been successfully deleted");
                    }
                }
            }
        }

        private Task<bool> TryDeleteDirectory(string dir, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(dir))
                {
                    // The directory does not exist, so it is already deleted.
                    return true;
                }

                if (!TryDeleteContentsAtomic(dir))
                {
                    this.logger.Error($"Failed to delete {dir} because it is in use.");
                    return false;
                }

                /*
                 * The directory is not in use, and the plugins system won't return that it is available
                 * to be loaded because we currently hold the lock on the plugin registry. We already deleted
                 * all of the files that were originally in the directory, so we just need to clean up the directory
                 * itself. Note that between deleting all the contents above and now, there could potentially be new
                 * files that were added (and which are now in use). Whatever those files are and whoever is using them
                 * is irrelevant to the plugins system, though, because the lock guarantees nobody is trying
                 * to load the plugin directory for plugin use.
                 *
                 * First, we will attempt to delete the directory immediately. If that fails, we will just (attempt to)
                 * rename the directory to a new, randomly generated, name.
                 */

                try
                {
                    Directory.Delete(dir, true);
                    return true;
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is UnauthorizedAccessException)
                {
                    this.logger.Info(ex, $"Failed to delete {dir}. Attempting to rename the folder.");
                }

                var di = new DirectoryInfo(dir);
                var moveTo = Path.Combine((di.Parent?.FullName ?? di.Root.FullName), Path.GetRandomFileName());

                try
                {
                    Directory.Move(dir, moveTo);
                }
                catch (IOException e)
                {
                    this.logger.Error(e, $"Failed to rename {dir} to {moveTo}.");
                    return false;
                }

                this.logger.Info($"Renamed {dir} to {moveTo}.");

                /*
                 * We failed to delete the original directory, so it's unlikely that we will be able to delete
                 * it after renaming it. The plugin won't be loaded or conflict with new installations since it's
                 * been renamed, though, so we'll just return true here and hope that the next time this cleanup code is
                 * ran the renamed directory is deleted.
                 */

                return true;
            }, cancellationToken);
        }

        private bool TryDeleteContentsAtomic(string directory)
        {
            List<string> files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).ToList();

            List<FileStream> streams = files
                .Select(f =>
                {
                    try
                    {
                        return File.Open(f, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(e, $"Failed to open {f} for deletion.");
                        return null;
                    }
                })
                .ToList();

            if (streams.Any(s => s is null))
            {
                // At least one file is in use. Close all the streams and return false.
                streams.ForEach(s =>
                {
                    s?.Close();
                    s?.Dispose();
                });

                return false;
            }

            // All files are available to be deleted. Delete all the files and close the streams.
            files.ForEach(f => File.Delete(f));
            streams.ForEach(s =>
            {
                s.Close();
                s.Dispose();
            });

            return true;
        }
    }
}
