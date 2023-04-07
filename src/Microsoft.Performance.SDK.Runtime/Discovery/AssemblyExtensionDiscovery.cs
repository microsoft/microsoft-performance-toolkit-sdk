// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

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
        private readonly Func<IEnumerable<string>, IPreloadValidator> validatorFactory;
        private readonly ILogger logger;
        private readonly List<IExtensionTypeObserver> observers = new List<IExtensionTypeObserver>();

        /// <summary>
        ///     Constructor takes an <see cref="IAssemblyLoader"/> which is used to do the actual assembly loading.
        /// </summary>
        /// <param name="assemblyLoader">
        ///     This is used to load individual assemblies.
        /// </param>
        /// <param name="validatorFactory">
        ///     Creates <see cref="IPreloadValidator"/> instances to make
        ///     sure candidate assemblies are valid to even try to load.
        ///     The function takes a collection of file names and returns
        ///     a new <see cref="IPreloadValidator"/> instance. This function
        ///     should never return <c>null</c>.
        /// </param>
        public AssemblyExtensionDiscovery(
            IAssemblyLoader assemblyLoader,
            Func<IEnumerable<string>, IPreloadValidator> validatorFactory)
            : this(assemblyLoader, validatorFactory, Logger.Create<AssemblyExtensionDiscovery>())
        {
        }

        /// <summary>
        ///     Constructor takes an <see cref="IAssemblyLoader"/> which is used to do the actual assembly loading.
        /// </summary>
        /// <param name="assemblyLoader">
        ///     This is used to load individual assemblies.
        /// </param>
        /// <param name="validatorFactory">
        ///     Creates <see cref="IPreloadValidator"/> instances to make
        ///     sure candidate assemblies are valid to even try to load.
        ///     The function takes a collection of file names and returns
        ///     a new <see cref="IPreloadValidator"/> instance. This function
        ///     should never return <c>null</c>.
        /// </param>
        /// <param name="logger">
        ///     Logs messages during loading.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="assemblyLoader" /> is <c>null</c>.
        ///     - or -
        ///     <paramref name="validatorFactory" /> is <c>null</c>.
        ///     - or -
        ///     <paramref name="logger" /> is <c>null</c>.
        /// </exception>
        public AssemblyExtensionDiscovery(
            IAssemblyLoader assemblyLoader,
            Func<IEnumerable<string>, IPreloadValidator> validatorFactory,
            ILogger logger)
        {
            Guard.NotNull(assemblyLoader, nameof(assemblyLoader));
            Guard.NotNull(validatorFactory, nameof(validatorFactory));
            Guard.NotNull(logger, nameof(logger));

            this.assemblyLoader = assemblyLoader;
            this.validatorFactory = validatorFactory;
            this.logger = logger;
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
        /// <param name="error">
        ///     If this method returns <c>false</c>, then this parameter receives
        ///     information about the error(s) that occurred.
        /// </param>
        /// <returns>
        ///     Whether or not all assemblies in the given directory were processed successfully.
        /// </returns>
        public bool ProcessAssemblies(string directoryPath, out ErrorInfo error)
        {
            return this.ProcessAssemblies(new[] { directoryPath, }, out error);
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers. It searches subdirectories for all "*.dll" and "*.exe" files.
        /// </summary>
        /// <param name="directoryPaths">
        ///     Directories to process.
        /// </param>
        /// <param name="error">
        ///     If this method returns <c>false</c>, then this parameter receives
        ///     information about the error(s) that occurred.
        /// </param>
        /// <returns>
        ///     Whether or not all assemblies in the given directories were processed successfully.
        /// </returns>
        public bool ProcessAssemblies(IEnumerable<string> directoryPaths, out ErrorInfo error)
        {
            return this.ProcessAssemblies(directoryPaths, true, null, null, MatchCasing.CaseInsensitive, out error);
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
        /// <param name="exclusionPatterns">
        ///     Files paths that match these patterns will not be processed.
        ///     May be <c>null</c>.
        ///     e.g. { "excludeMe*.dll", "blob.dll" }
        /// </param>
        /// <param name="fileNameCaseSensitivity">
        ///     Indicates whether files name pattern matching should be case sensitive.
        /// </param>
        /// <param name="error">
        ///     If this method returns <c>false</c>, then this parameter receives
        ///     information about the error(s) that occurred.
        /// </param>
        /// <returns>
        ///     Whether or not all assemblies in the given directory were processed successfully.
        /// </returns>
        public bool ProcessAssemblies(
            string directoryPath,
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionPatterns,
            MatchCasing fileNameCaseSensitivity,
            out ErrorInfo error)
        {
            return this.ProcessAssemblies(
                new[] { directoryPath, },
                includeSubdirectories,
                searchPatterns,
                exclusionPatterns,
                fileNameCaseSensitivity,
                out error);
        }

        /// <summary>
        ///     This method scans the given directory for modules to search for extensibility types. All types found will
        ///     be passed along to any observers.
        /// </summary>
        /// <remarks>
        ///     For more on search patterns, see <seealso cref="Directory.EnumerateFiles(string, string)"/>.
        /// </remarks>
        /// <param name="directoryPaths">
        ///     Directories to process.
        /// </param>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns to use. If null or empty, defaults to "*.dll" and "*.exe".
        /// </param>
        /// <param name="exclusionPatterns">
        ///     Files paths that match these patterns will not be processed.
        ///     May be <c>null</c>.
        ///     e.g. { "excludeMe*.dll", "blob.dll" }
        /// </param>
        /// <param name="fileNameCaseSensitivity">
        ///     Indicates whether files name pattern matching should be case sensitive.
        /// </param>
        /// <param name="error">
        ///     If this method returns <c>false</c>, then this parameter receives
        ///     information about the error(s) that occurred.
        /// </param>
        /// <returns>
        ///     Whether or not all assemblies in the given directories were processed successfully.
        /// </returns>
        public bool ProcessAssemblies(
            IEnumerable<string> directoryPaths,
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionPatterns,
            MatchCasing fileNameCaseSensitivity,
            out ErrorInfo error)
        {
            Guard.NotNull(directoryPaths, nameof(directoryPaths));
            directoryPaths.ForEach(x => Guard.NotNullOrWhiteSpace(x, nameof(directoryPaths)));

            Parallel.ForEach(this.observers, (observer) => observer.DiscoveryStarted());

            var watch = Stopwatch.StartNew();

            if (searchPatterns == null || !searchPatterns.Any())
            {
                searchPatterns = new[] { "*.dll", "*.exe" };
            }

            var enumerationOptions = new EnumerationOptions();
            enumerationOptions.RecurseSubdirectories = includeSubdirectories;
            enumerationOptions.MatchCasing = fileNameCaseSensitivity;

            var loaded = new List<Assembly>();

            bool allLoaded = true;

            var directoryErrors = new List<ErrorInfo>();
            lock (this.observers)
            {
                if (!this.observers.Any())
                {
                    // If a tree falls in a forest and no one is around to hear it, does it make a sound?
                    error = new ErrorInfo(ErrorCodes.NoObserversRegistered, "No observers are registered.");
                    return false;
                }

                foreach (var directoryPath in directoryPaths)
                {
                    var assemblyErrors = new List<ErrorInfo>();
                    if (!Directory.Exists(directoryPath))
                    {
                        directoryErrors.Add(
                            new ErrorInfo(ErrorCodes.DirectoryNotFound, ErrorCodes.DirectoryNotFound.Description)
                            {
                                Target = directoryPath,
                            });
                        allLoaded = false;
                        continue;
                    }

                    HashSet<string> inclusionFiles = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var searchPattern in searchPatterns)
                    {
                        this.FindFiles.EnumerateFiles(directoryPath, searchPattern, enumerationOptions)
                            .ForEach(x => inclusionFiles.Add(x));
                    }

                    HashSet<string> exclusionFiles = new HashSet<string>(StringComparer.Ordinal);
                    if (exclusionPatterns != null)
                    {
                        foreach (var searchPattern in exclusionPatterns)
                        {
                            this.FindFiles.EnumerateFiles(directoryPath, searchPattern, enumerationOptions)
                                .ForEach(x => exclusionFiles.Add(x));
                        }
                    }

                    string[] filePaths = inclusionFiles.Except(exclusionFiles).ToArray();
                    using (var validator = this.validatorFactory(filePaths))
                    {
                        if (validator is null)
                        {
                            Debug.Fail("Validator factory should never return null");
                            allLoaded = false;
                            assemblyErrors.Add(
                                new ErrorInfo(
                                    ErrorCodes.InvalidArgument,
                                    "The preload validator factory returned null, which is not valid."));
                        }

                        loaded.EnsureCapacity(loaded.Capacity + filePaths.Length);

                        foreach (var filePath in filePaths.Where(this.assemblyLoader.IsAssembly))
                        {
                            if (validator.IsAssemblyAcceptable(filePath, out var validationError))
                            {
                                var assembly = this.assemblyLoader.LoadAssembly(filePath, out var loadError);
                                if (!(assembly is null))
                                {
                                    if (!ProcessAssembly(assembly, out var assemblyError))
                                    {
                                        allLoaded = false;
                                        Debug.Assert(assemblyError != null);
                                        assemblyErrors.Add(assemblyError);
                                    }
                                }
                                else
                                {
                                    allLoaded = false;
                                    assemblyErrors.Add(loadError);
                                }
                            }
                            else
                            {
                                allLoaded = false;
                                assemblyErrors.Add(validationError);
                            }
                        }
                    }

                    if (assemblyErrors.Count > 0)
                    {
                        directoryErrors.Add(
                            new ErrorInfo(ErrorCodes.AssemblyLoadFailed, ErrorCodes.AssemblyLoadFailed.Description)
                            {
                                Target = directoryPath,
                                Details = assemblyErrors.ToArray(),
                            });
                    }
                }

                Parallel.ForEach(this.observers, (observer) => observer.DiscoveryComplete());
            }

            watch.Stop();
            this.logger.Info("Loaded {0} in {1}ms", allLoaded ? "all" : "some", watch.ElapsedMilliseconds);

            if (directoryErrors.Count > 0)
            {
                Debug.Assert(!allLoaded);
                error = new DiscoveryError(
                            directoryPaths,
                            includeSubdirectories,
                            searchPatterns,
                            exclusionPatterns,
                            fileNameCaseSensitivity)
                {
                    Target = string.Empty,
                    Details = directoryErrors.ToArray(),
                };
            }
            else
            {
                Debug.Assert(allLoaded);
                error = null;
            }

            return allLoaded;
        }

        private bool ProcessAssembly(
            Assembly assembly,
            out ErrorInfo error)
        {
            Guard.NotNull(assembly, nameof(assembly));

            if (!assembly.ReferencesSdk())
            {
                //
                // Without an SDK reference, there cannot be plugins
                // or extensions in the assembly, so we can save some
                // work by just skipping it.
                //

                error = ErrorInfo.None;
                return true;
            }

            var assemblyName = assembly.GetName().FullName;

            Type[] assemblyTypes = null;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                error = new ErrorInfo(
                    ErrorCodes.UnableToReflectAssemblyTypes,
                    "Unable to enumerate the types in the assembly.")
                {
                    Target = assembly.FullName,
                    Details = e.LoaderExceptions
                        .Select(
                            x => new ErrorInfo(ErrorCodes.LoaderException, e.Message)
                            {
                                Target = assembly.FullName,
                                Details = new[] { FusionError.TryCreate(assembly.FullName, x), },
                            })
                        .ToArray(),
                };

                return false;
            }

            Debug.Assert(assemblyTypes != null);

            var processingErrors = new List<ErrorInfo>();
            foreach (var type in assemblyTypes)
            {
                var observerErrors = new ConcurrentBag<(IExtensionTypeObserver, Exception)>();
                Parallel.ForEach(observers, (observer) =>
                {
                    try
                    {
                        observer.ProcessType(type, assemblyName);
                    }
                    catch (Exception e)
                    {
                        observerErrors.Add((observer, e));
                    }
                });

                if (observerErrors.Count > 0)
                {
                    foreach (var oe in observerErrors)
                    {
                        processingErrors.Add(
                            new ErrorInfo(ErrorCodes.TypeInspectionFailure, oe.Item2.Message)
                            {
                                Target = type.FullName,
                            });
                    }
                }
            }

            if (processingErrors.Count > 0)
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    "The assembly for the plugin failed to be inspected.")
                {
                    Target = assembly.FullName,
                    Details = processingErrors.ToArray(),
                };

                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        ///     This is used to enable unit testing by avoiding the actual file system.
        /// </summary>
        internal interface IFindFiles
        {
            IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, EnumerationOptions enumerationOption);
        }

        /// <summary>
        ///     This provides the default behavior in all but the unit test case.
        /// </summary>
        private class DirectorySearch : IFindFiles
        {
            public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, EnumerationOptions enumerationOption)
            {
                return Directory.EnumerateFiles(directoryPath, searchPattern, enumerationOption);
            }
        }

        [Serializable]
        private class FusionError
            : ErrorInfo,
              ISerializable
        {
            public FusionError(
                FileNotFoundException e)
                : base(ErrorCodes.AssemblyLoadFailed, ErrorCodes.AssemblyLoadFailed.Description)
            {
                this.Exception = e;
            }

            protected FusionError(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
            {
                this.Exception = (FileNotFoundException)info.GetValue(
                    nameof(this.Exception),
                    typeof(FileNotFoundException));
            }

            public FileNotFoundException Exception { get; }

            public string FileName => this.Exception.FileName;

            public string FusionLog => this.Exception.FusionLog;

            public static ErrorInfo TryCreate(string target, Exception loaderException)
            {
                if (loaderException is FileNotFoundException fnfe)
                {
                    return new FusionError(fnfe);
                }

                return null;
            }

            [SecurityPermission(
                SecurityAction.LinkDemand,
                Flags = SecurityPermissionFlag.SerializationFormatter)]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue(nameof(this.Exception), this.Exception);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                this.GetObjectData(info, context);
            }
        }

        [Serializable]
        private class DiscoveryError
            : ErrorInfo,
              ISerializable
        {
            public DiscoveryError(
                IEnumerable<string> directoryPaths,
                bool includeSubdirectories,
                IEnumerable<string> searchPatterns,
                IEnumerable<string> exclusionFileNames,
                MatchCasing fileMatchCaseSensitivity)
                : base(ErrorCodes.DiscoveryFailed, ErrorCodes.DiscoveryFailed.Description)
            {
                this.DirectoryPaths = directoryPaths?.ToList() ?? new List<string>();
                this.IncludeSubDirectories = includeSubdirectories;
                this.SearchPatterns = searchPatterns?.ToList() ?? new List<string>();
                this.ExclusionFileNames = exclusionFileNames?.ToList() ?? new List<string>();
                this.FileMatchCaseSensitivity = fileMatchCaseSensitivity;
            }

            protected DiscoveryError(
               SerializationInfo info,
               StreamingContext context)
               : base(info, context)
            {
                this.DirectoryPaths = (List<string>)info.GetValue(nameof(this.DirectoryPaths), typeof(List<string>));
                this.IncludeSubDirectories = info.GetBoolean(nameof(this.IncludeSubDirectories));
                this.SearchPatterns = (List<string>)info.GetValue(nameof(this.SearchPatterns), typeof(List<string>));
                this.ExclusionFileNames = (List<string>)info.GetValue(nameof(this.ExclusionFileNames), typeof(List<string>));
                this.FileMatchCaseSensitivity = (MatchCasing)info.GetValue(nameof(this.FileMatchCaseSensitivity), typeof(MatchCasing));
            }

            public List<string> DirectoryPaths { get; }

            public bool IncludeSubDirectories { get; }

            public List<string> SearchPatterns { get; }

            public List<string> ExclusionFileNames { get; }

            public MatchCasing FileMatchCaseSensitivity { get; }

            [SecurityPermission(
                SecurityAction.LinkDemand,
                Flags = SecurityPermissionFlag.SerializationFormatter)]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue(nameof(this.DirectoryPaths), this.DirectoryPaths);
                info.AddValue(nameof(this.IncludeSubDirectories), this.IncludeSubDirectories);
                info.AddValue(nameof(this.SearchPatterns), this.SearchPatterns);
                info.AddValue(nameof(this.ExclusionFileNames), this.ExclusionFileNames);
                info.AddValue(nameof(this.FileMatchCaseSensitivity), this.FileMatchCaseSensitivity);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                this.GetObjectData(info, context);
            }
        }
    }
}
