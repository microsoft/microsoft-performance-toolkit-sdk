// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     This class loads Assemblies and passes the types found in the assemblies to registered observers to check for
    ///     possible extensions to the SDK.
    /// </summary>
    public class AssemblyExtensionDiscovery
        : IExtensionTypeProvider
    {
        private readonly IAssemblyLoader assemblyLoader;
        private readonly List<IExtensionTypeObserver> observers = new List<IExtensionTypeObserver>();

        /// <summary>
        ///     Constructor takes an <see cref="IAssemblyLoader"/> which is used to do the actual assembly loading.
        /// </summary>
        /// <param name="assemblyLoader">
        ///     This is used to load individual assemblies.
        /// </param>
        public AssemblyExtensionDiscovery(IAssemblyLoader assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
        }

        /// <summary>
        ///     Only unit tests are expected to change this value.
        /// </summary>
        internal IFindFiles FindFiles { get; set; } = new DirectorySearch();

        /// <summary>
        ///     Called to register to receive notification of loaded types from possible extension modules.
        /// </summary>
        /// <param name="observer">
        ///     The object that receives updates of loaded types.
        /// </param>
        public void RegisterTypeConsumer(IExtensionTypeObserver observer)
        {
            lock (this.observers)
            {
                this.observers.Add(observer);
            }
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers. It searches subdirectories for all "*.dll" and "*.exe" files.
        /// </summary>
        /// <param name="directoryPath">
        ///     Directory to process.
        /// </param>
        public void ProcessAssemblies(string directoryPath)
        {
            this.ProcessAssemblies(new[] { directoryPath, });
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers. It searches subdirectories for all "*.dll" and "*.exe" files.
        /// </summary>
        /// <param name="directoryPaths">
        ///     Directories to process.
        /// </param>
        public void ProcessAssemblies(IEnumerable<string> directoryPaths)
        {
            this.ProcessAssemblies(directoryPaths, true, null, null, false);
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers.
        /// </summary>
        /// <param name="directoryPath">
        ///     Directory to process.
        /// </param>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns to use. If null or empty, defaults to "*.dll" and "*.exe".
        /// </param>
        /// <param name="exclusionFileNames">
        ///     A set of files names to exclude from the search. May be null.
        /// </param>
        /// <param name="exclusionsAreCaseSensitive">
        ///     Indicates whether files names should be treated as case sensitive.
        /// </param>
        public void ProcessAssemblies(
            string directoryPath,
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionFileNames,
            bool exclusionsAreCaseSensitive)
        {
            this.ProcessAssemblies(
                new[] { directoryPath, },
                includeSubdirectories,
                searchPatterns,
                exclusionFileNames,
                exclusionsAreCaseSensitive);
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers.
        /// </summary>        
        /// <param name="directoryPaths">
        ///     Directories to process.
        /// </param>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns to use. If null or empty, defaults to "*.dll" and "*.exe".
        /// </param>
        /// <param name="exclusionFileNames">
        ///     A set of files names to exclude from the search. May be null.
        /// </param>
        /// <param name="exclusionsAreCaseSensitive">
        ///     Indicates whether files names should be treated as case sensitive.
        /// </param>
        public void ProcessAssemblies(
            IEnumerable<string> directoryPaths,
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionFileNames,
            bool exclusionsAreCaseSensitive)
        {
            Guard.NotNull(directoryPaths, nameof(directoryPaths));
            directoryPaths.ForEach(x => Guard.NotNullOrWhiteSpace(x, nameof(directoryPaths)));

            var watch = Stopwatch.StartNew();

            if (searchPatterns == null || !searchPatterns.Any())
            {
                searchPatterns = new[] { "*.dll", "*.exe" };
            }

            var exclusionSet = exclusionFileNames != null
                ? new HashSet<string>(exclusionFileNames, exclusionsAreCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var loaded = new List<Assembly>();

            lock (this.observers)
            {
                if (!this.observers.Any())
                {
                    // If a tree falls in a forest and no one is around to hear it, does it make a sound?
                    return;
                }

                foreach (var directoryPath in directoryPaths)
                {
                    foreach (var searchPattern in searchPatterns)
                    {
                        var filePaths = this.FindFiles.EnumerateFiles(directoryPath, searchPattern, searchOption)
                            .Where(x => !exclusionSet.Contains(Path.GetFileName(x)))
                            .ToArray();

                        if (filePaths.Length == 0)
                        {
                            continue;
                        }

                        loaded.EnsureCapacity(loaded.Capacity + filePaths.Length);

                        foreach (var filePath in filePaths)
                        {
                            var assembly = this.assemblyLoader.LoadAssembly(filePath);
                            if (!(assembly is null))
                            {
                                ProcessAssembly(assembly);
                            }
                        }
                    }
                }

                Parallel.ForEach(this.observers, (observer) => observer.DiscoveryComplete());
            }

            watch.Stop();
            Console.Error.WriteLine("Loaded all in {0}", watch.Elapsed);
        }

        private void ProcessAssembly(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            var assemblyName = assembly.GetName().FullName;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    Parallel.ForEach(observers, (observer) =>
                    {
                        observer.ProcessType(type, assemblyName);
                    });
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.Error.WriteLine("Unable to examine `{0}`: ", assembly.GetName());
                Console.Error.WriteLine("--> {0}", e.Message);
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Console.Error.WriteLine("----> {0}", loaderException.Message);
                    if (loaderException is FileNotFoundException fnfe)
                    {
                        Console.Error.WriteLine("------> {0}", fnfe.FusionLog);
                    }

                    Console.Error.WriteLine(loaderException.InnerException);
                }
            }
            catch (TargetInvocationException e)
            {
                Console.Error.WriteLine("Unable to examine `{0}`: ", assembly.GetName());
                Console.Error.WriteLine("--> {0}", e.Message);
                Console.Error.WriteLine("----> {0}", e.InnerException?.Message ?? "<inner exception was null>");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unable to examine `{0}`: ", assembly.GetName());
                Console.Error.WriteLine("--> {0}", e.Message);
            }
        }

        /// <summary>
        ///     This is used to enable unit testing by avoiding the actual file system.
        /// </summary>
        internal interface IFindFiles
        {
            IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption);
        }

        /// <summary>
        ///     This provides the default behavior in all but the unit test case.
        /// </summary>
        private class DirectorySearch : IFindFiles
        {
            public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption)
            {
                return Directory.EnumerateFiles(directoryPath, searchPattern, searchOption);
            }
        }
    }
}
