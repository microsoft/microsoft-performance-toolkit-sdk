// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;

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

                // First try to rename the directory to a temporary name.
                // If this succeeds, it means no other process was using any file
                // in the directory, and we can safely delete it.
                var toDelete = dir + ".delete";
                try
                {
                    Directory.Move(dir, toDelete);
                }
                catch (IOException e)
                {
                    this.logger.Error(e, $"Failed to delete {dir} because it is in use.");
                    return false;
                }

                try
                {
                    Directory.Delete(toDelete, true);
                    return true;
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is UnauthorizedAccessException)
                {
                    this.logger.Error(ex, $"Failed to delete {dir}.");

                    // Restore the original directory name
                    Directory.Move(toDelete, dir);
                    return false;
                }
            }, cancellationToken);
        }

    }
}
