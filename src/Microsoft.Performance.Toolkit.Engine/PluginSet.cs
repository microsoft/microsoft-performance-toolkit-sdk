// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents a collection of plugins.
    /// </summary>
    public sealed class PluginSet
        : IDisposable
    {
        private readonly IEnumerable<string> extensionDirectories;
        private readonly ExtensionRoot extensions;
        private readonly DataExtensionFactory factory;
        private readonly IEnumerable<ErrorInfo> creationErrors;
        private readonly bool arePluginsIsolated;

        private bool isDisposed;

        internal PluginSet(
            IEnumerable<string> extensionDirectories,
            ExtensionRoot extensions,
            DataExtensionFactory factory,
            IEnumerable<ErrorInfo> creationErrors,
            bool arePluginsIsolated)
        {
            Debug.Assert(extensionDirectories != null);
            Debug.Assert(extensionDirectories.Any());
            Debug.Assert(!extensionDirectories.Any(string.IsNullOrWhiteSpace));
            Debug.Assert(extensions != null);
            Debug.Assert(factory != null);

            this.extensionDirectories = extensionDirectories.ToList().AsReadOnly();
            this.extensions = extensions;
            this.factory = factory;
            this.creationErrors = creationErrors ?? Enumerable.Empty<ErrorInfo>();
            this.arePluginsIsolated = arePluginsIsolated;
        }

        /// <summary>
        ///     Gets a value indicating whether plugins are isolated into their
        ///     own isolated assembly contexts.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool ArePluginsIsolated
        {
            get
            {
                this.ThrowIfDisposed();
                return this.arePluginsIsolated;
            }
        }

        /// <summary>
        ///     Gets any non-fatal errors that occured during data set
        ///     creation. Because these errors are not fatal, they are
        ///     reported here rather than raising an exception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<ErrorInfo> CreationErrors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.creationErrors;
            }
        }

        /// <summary>
        ///     Gets the directories from which the plugins in this set were loaded.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<string> ExtensionDirectories
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDirectories;
            }
        }

        /// <summary>
        ///     Gets all of the loaded Processing Sources.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<ProcessingSourceReference> ProcessingSourceReferences => this.Extensions.ProcessingSources;

        /// <summary>
        ///     Gets the paths of all loaded Source Cookers.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> SourceDataCookers => this.Extensions.SourceDataCookers;

        /// <summary>
        ///     Gets the paths of all loaded Composite Cookers.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> CompositeDataCookers => this.Extensions.CompositeDataCookers;

        /// <summary>
        ///     Gets the paths of all loaded cookers.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> AllCookers => this.SourceDataCookers.Concat(this.CompositeDataCookers);

        /// <summary>
        ///     Gets the mapping of all loaded Table Ids to the corresponding loaded Table.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById => this.Extensions.TablesById;

        // TODO: __SDK_DP__
        // Redesign Data Processor API
        /////// <summary>
        ///////     Gets the paths of all loaded Data Processors.
        /////// </summary>
        /////// <exception cref="ObjectDisposedException">
        ///////     This instance is disposed.
        /////// </exception>
        ////public IEnumerable<DataProcessorId> DataProcessors => this.Extensions.DataProcessors;

        internal ExtensionRoot Extensions
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensions;
            }
        }

        internal DataExtensionFactory Factory
        {
            get
            {
                this.ThrowIfDisposed();
                return this.factory;
            }
        }

        /// <summary>
        ///     Creates a new <see cref="PluginSet"/>, loading all plugins found
        ///     in the current working directory.
        /// </summary>
        /// <returns>
        ///     A new instance of the <see cref="PluginSet"/> class containing all
        ///     of the successfully discovered plugins.
        /// </returns>
        public static PluginSet Load()
        {
            return Load(Environment.CurrentDirectory);
        }

        /// <summary>
        ///     Creates a new <see cref="PluginSet"/>, loading all plugins found
        ///     in the given directory.
        /// </summary>
        /// <param name="extensionDirectory">
        ///     The path to the directory search for extensions.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="PluginSet"/> class containing all
        ///     of the successfully discovered plugins.
        /// </returns>
        /// <exception cref="InvalidExtensionDirectoryException">
        ///     <paramref name="extensionDirectory"/> is invalid or does not exist.
        /// </exception>
        public static PluginSet Load(string extensionDirectory)
        {
            return Load(new[] { extensionDirectory, });
        }

        /// <summary>
        ///     Creates a new <see cref="PluginSet"/>, loading all plugins found
        ///     in the given directories.
        /// </summary>
        /// <param name="extensionDirectories">
        ///     The directories to search for plugins.
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="PluginSet"/> class containing all
        ///     of the successfully discovered plugins.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="extensionDirectories"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="extensionDirectories"/> is empty.
        /// </exception>
        /// <exception cref="InvalidExtensionDirectoryException">
        ///     One or more directory paths in <paramref name="extensionDirectories"/>
        ///     is invalid or does not exist.
        /// </exception>
        public static PluginSet Load(
            IEnumerable<string> extensionDirectories)
        {
            return Load(extensionDirectories, null);
        }
        /// <summary>
        ///     Creates a new <see cref="PluginSet"/>, loading all plugins found
        ///     in the given directories, using the given loading function.
        /// </summary>
        /// <param name="extensionDirectories">
        ///     The directories to search for plugins.
        /// </param>
        /// <param name="assemblyLoader">
        ///     The loader to use to load plugin assemblies. This parameter may be
        ///     <c>null</c>. If this parameter is <c>null</c>, then the default
        ///     loader will be used.
        ///     <remarks>
        ///         The default loader provides no isolation.
        ///     </remarks>
        /// </param>
        /// <returns>
        ///     A new instance of the <see cref="PluginSet"/> class containing all
        ///     of the successfully discovered plugins. The returned instance will
        ///     also contain a collection of non-fatal errors that occurred when
        ///     creating this data set (e.g. a plugin failed to load.)
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="extensionDirectories"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="extensionDirectories"/> is empty.
        /// </exception>
        /// <exception cref="InvalidExtensionDirectoryException">
        ///     One or more directory paths in <paramref name="extensionDirectories"/>
        ///     is invalid or does not exist.
        /// </exception>
        public static PluginSet Load(
            IEnumerable<string> extensionDirectories,
            IAssemblyLoader assemblyLoader)
        {
            Guard.NotNull(extensionDirectories, nameof(extensionDirectories));
            Guard.Any(extensionDirectories, nameof(extensionDirectories));

            IProcessingSourceCatalog catalog = null;
            IDataExtensionRepositoryBuilder repo = null;
            ExtensionRoot extensionRoot = null;

            var extensionDirectoriesFullPaths = new List<string>();

            foreach (var directory in extensionDirectories)
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    throw new InvalidExtensionDirectoryException(directory);
                }

                DirectoryInfo dirInfo;
                try
                {
                    dirInfo = new DirectoryInfo(directory);
                }
                catch (Exception e)
                {
                    throw new InvalidExtensionDirectoryException(directory, e);
                }

                if (!dirInfo.Exists)
                {
                    throw new InvalidExtensionDirectoryException(directory);
                }

                extensionDirectoriesFullPaths.Add(dirInfo.FullName);
            }

            try
            {
                assemblyLoader ??= new AssemblyLoader();
                var validatorFactory = new Func<IEnumerable<string>, IPreloadValidator>(_ => new NullValidator());

                var assemblyDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, validatorFactory);

                catalog = new ReflectionProcessingSourceCatalog(assemblyDiscovery);

                var factory = new DataExtensionFactory();
                repo = factory.CreateDataExtensionRepository();

                var reflector = new DataExtensionReflector(assemblyDiscovery, repo);

                assemblyDiscovery.ProcessAssemblies(extensionDirectoriesFullPaths, out var discoveryError);

                repo.FinalizeDataExtensions();

                var creationErrors = discoveryError != null && discoveryError != ErrorInfo.None
                    ? new[] { discoveryError, }
                    : Array.Empty<ErrorInfo>();

                extensionRoot = new ExtensionRoot(catalog, repo);

                return new PluginSet(
                    extensionDirectoriesFullPaths,
                    extensionRoot,
                    factory,
                    creationErrors,
                    assemblyLoader.SupportsIsolation);
            }
            catch (Exception)
            {
                extensionRoot.SafeDispose();
                repo.SafeDispose();
                catalog.SafeDispose();
                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.extensions.SafeDispose();
            }

            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(PluginSet));
            }
        }        

        private sealed class NullValidator
            : IPreloadValidator
        {
            public bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error)
            {
                error = ErrorInfo.None;
                return true;
            }

            public void Dispose()
            {
            }
        }
    }
}
