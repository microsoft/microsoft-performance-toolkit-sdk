// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery
{
    /// <summary>
    ///     Used to load assemblies into isolated contexts.
    ///     <para/>
    ///     Assemblies loaded through this class do not have any security
    ///     boundary guarantees. For more information, refer to <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext?view=netcore-3.1"/>
    /// </summary>
    public sealed class IsolationAssemblyLoader
        : AssemblyLoaderBase
    {
        // filename, name
        private static readonly IReadOnlyDictionary<string, string> AlwaysSharedAssembliesFileNameToName
            = new Dictionary<string, string>
        {
            { "Microsoft.Performance.SDK.dll", "Microsoft.Performance.SDK" },
            { "Microsoft.Performance.SDK.Runtime.dll", "Microsoft.Performance.SDK.Runtime" },
            { "Microsoft.Performance.SDK.Runtime.NetCoreApp.dll", "Microsoft.Performance.SDK.Runtime.NetCoreApp" },

            // The type SemanticVersion is exposed publicly and so this assembly must be shared.
            { "NuGet.Versioning.dll", "NuGet.Versioning" },
        };

        private static readonly Lazy<string> ApplicationBase = new Lazy<string>(
            () => Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
            System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly ConcurrentDictionary<string, AssemblyLoadContext> loadContexts =
            new ConcurrentDictionary<string, AssemblyLoadContext>();

        // This is  used for testing purposes only
        internal static readonly IReadOnlyDictionary<string, AssemblyLoadContext> LoadContexts =
            new ReadOnlyDictionary<string, AssemblyLoadContext>(loadContexts);

        private readonly Dictionary<string, string> alwaysSharedAssembliesFileNameToName;
        private readonly Dictionary<string, string> alwaysSharedAssembliesNameToFileName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IsolationAssemblyLoader"/>
        ///     class.
        /// </summary>
        public IsolationAssemblyLoader()
            : this(new Dictionary<string, string>(), Runtime.Logger.Create<IsolationAssemblyLoader>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IsolationAssemblyLoader"/>
        ///     class, denoting any additional assemblies that must always be shered
        ///     between the enclosing application and plugins.
        /// </summary>
        /// <param name="additionalSharedAssembliesFileNameToName">
        ///     The additional assemblies, if any, are specified by dictionary mapping
        ///     the file name of the assembly (the file name with the extension) to
        ///     the name of the assembly. For example, { "Microsoft.Performance.SDK.dll", "Microsoft.Performance.SDK" }
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="logger">
        ///     Logs messages during loading.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public IsolationAssemblyLoader(
            IDictionary<string, string> additionalSharedAssembliesFileNameToName,
            ILogger logger)
            : base(logger)
        {
            Guard.NotNull(logger, nameof(logger));

            this.alwaysSharedAssembliesFileNameToName = new Dictionary<string, string>(AlwaysSharedAssembliesFileNameToName);

            if (!(additionalSharedAssembliesFileNameToName is null))
            {
                foreach (var kvp in additionalSharedAssembliesFileNameToName)
                {
                    this.alwaysSharedAssembliesFileNameToName.TryAdd(kvp.Key, kvp.Value);
                }
            }

            this.alwaysSharedAssembliesNameToFileName = this.alwaysSharedAssembliesFileNameToName.ToDictionary(
                x => x.Value,
                x => x.Key);
        }

        /// <inheritdoc />
        public override bool SupportsIsolation => true;

        /// <inheritdoc />
        protected override Assembly LoadFromPath(string assemblyPath)
        {
            var directory = Path.GetDirectoryName(assemblyPath);
            var fileName = Path.GetFileName(assemblyPath);

            var loadedAssembly = this.SpeciallyLoadedByAssemblyFileName(fileName);
            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            var loadContext = loadContexts.GetOrAdd(directory, x => new PluginAssemblyLoadContext(x, this.SpeciallyLoadedByAssemblyName));

            Debug.Assert(loadContext != null);

            return loadContext.LoadFromAssemblyPath(assemblyPath);
        }

        // This is  used for testing purposes only
        internal static void ClearLoadContexts()
        {
            foreach (var kvp in loadContexts)
            {
                try
                {
                    kvp.Value.Unload();
                }
                catch (Exception)
                {
                }
            }

            loadContexts.Clear();
        }

        private Assembly SpeciallyLoadedByAssemblyName(string assemblyName)
        {
            if (!this.alwaysSharedAssembliesNameToFileName.TryGetValue(assemblyName, out var assemblyFileName))
            {
                return null;
            }

            return this.SpeciallyLoadedByAssemblyFileName(assemblyFileName);
        }

        private Assembly SpeciallyLoadedByAssemblyFileName(string fileName)
        {
            if (!this.alwaysSharedAssembliesFileNameToName.ContainsKey(fileName))
            {
                return null;
            }

            var loadedAssembly = AssemblyLoadContext.Default.Assemblies.SingleOrDefault(
                x => !x.IsDynamic &&
                     StringComparer.OrdinalIgnoreCase.Equals(
                         Path.GetFileName(x.GetCodeBaseAsLocalPath()),
                         fileName));

            if (loadedAssembly == null)
            {
                var assemblyPath = Path.Combine(ApplicationBase.Value, fileName);

                Debug.Assert(File.Exists(assemblyPath));

                loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            }

            return loadedAssembly;
        }

        /// <summary>
        ///     This is used to find references to other modules from modules exposed within an
        ///     <see cref="IsolationAssemblyLoader"/>. If the referenced module doesn't already exist in the given context, the
        ///     AssemblyLoadContext will call <see cref="Load"/> to try to find it.
        /// </summary>
        private sealed class PluginAssemblyLoadContext
            : AssemblyLoadContext
        {
            private readonly string pluginDirectory;
            private readonly Func<string, Assembly> speciallyLoadedByAssemblyName;

            // todo: I think we need to include *.deps.json for extensions to make sure the resolvers work correctly
            // see: https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
            private readonly List<AssemblyDependencyResolver> resolvers;

            public PluginAssemblyLoadContext(
                string directory,
                Func<string, Assembly> speciallyLoadedByAssemblyName)
                : base("Plugin: " + directory)
            {
                Guard.NotNullOrWhiteSpace(directory, nameof(directory));
                Guard.NotNull(speciallyLoadedByAssemblyName, nameof(speciallyLoadedByAssemblyName));

                this.pluginDirectory = directory;
                this.speciallyLoadedByAssemblyName = speciallyLoadedByAssemblyName;
                this.resolvers = new List<AssemblyDependencyResolver>();

                //
                // A plugin will only load dependencies from:
                //  - it's directory tree
                //  - the shared context.
                //

                // todo: when debugging, it looks like we get a lot of dups in this loop - worth some more investigation at some point
                // I think *maybe* we only need to add resolvers if there is an associated *.deps.json file, and otherwise we'll fall
                // back to the FindFileInDirectory approach?
                var dlls = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);
                foreach (var dll in dlls)
                {
                    this.resolvers.Add(new AssemblyDependencyResolver(dll));
                }
            }

            protected override Assembly Load(
                AssemblyName assemblyName)
            {
                //
                // Check if the requested assembly is a shared assembly.
                //
                Assembly loadedAssembly = this.speciallyLoadedByAssemblyName(assemblyName.Name);
                if (loadedAssembly is null)
                {
                    //
                    // Use the standard resolution rules.
                    //

                    foreach (var resolver in this.resolvers)
                    {
                        var path = resolver.ResolveAssemblyToPath(assemblyName);
                        if (path != null)
                        {
                            loadedAssembly = this.LoadFromAssemblyPath(path);
                            break;
                        }
                    }

                    //
                    // We didn't find the assembly anywhere, so probe the plugin's 
                    // entire directory tree to see if we can find it in any of the
                    // subfolders.
                    //

                    if (loadedAssembly is null)
                    {
                        var name = assemblyName.Name;
                        var filePath = FindFileInDirectory(name, pluginDirectory);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            loadedAssembly = this.LoadFromAssemblyPath(filePath);
                        }
                    }
                }

                return loadedAssembly;
            }

            protected override IntPtr LoadUnmanagedDll(
                string unmanagedDllName)
            {
                //
                // Use the standard resolution rules.
                //

                IntPtr found = IntPtr.Zero;
                foreach (var resolver in this.resolvers)
                {
                    var path = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                    if (path != null)
                    {
                        found = this.LoadUnmanagedDllFromPath(path);
                        break;
                    }
                }

                //
                // We didn't find the DLL anywhere, so probe the plugin's 
                // entire directory tree to see if we can find it in any of the
                // subfolders.
                //

                if (found == IntPtr.Zero)
                {
                    var filePath = FindFileInDirectory(unmanagedDllName, pluginDirectory);
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        found = this.LoadUnmanagedDllFromPath(filePath);
                    }
                }

                return found;
            }

            private static string FindFileInDirectory(
                string fileNameWithoutExtension,
                string directory)
            {
                foreach (var filePath in Directory.GetFiles(directory))
                {
                    var candidate = Path.GetFileNameWithoutExtension(filePath);
                    if (fileNameWithoutExtension == candidate)
                    {
                        return filePath;
                    }
                }

                foreach (var directoryPath in Directory.GetDirectories(directory))
                {
                    var filePath = FindFileInDirectory(
                        fileNameWithoutExtension,
                        directoryPath);
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        return filePath;
                    }
                }

                return null;
            }
        }
    }
}
