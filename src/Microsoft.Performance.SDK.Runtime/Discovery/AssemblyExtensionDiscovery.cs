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
using System.Text;
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
        private readonly Func<IEnumerable<string>, IPreloadValidator> validatorFactory;
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
        {
            this.assemblyLoader = assemblyLoader;
            this.validatorFactory = validatorFactory;
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
            return this.ProcessAssemblies(directoryPaths, true, null, null, false, out error);
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
            IEnumerable<string> exclusionFileNames,
            bool exclusionsAreCaseSensitive,
            out ErrorInfo error)
        {
            return this.ProcessAssemblies(
                new[] { directoryPath, },
                includeSubdirectories,
                searchPatterns,
                exclusionFileNames,
                exclusionsAreCaseSensitive,
                out error);
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
            IEnumerable<string> exclusionFileNames,
            bool exclusionsAreCaseSensitive,
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

            var exclusionSet = exclusionFileNames != null
                ? new HashSet<string>(exclusionFileNames, exclusionsAreCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

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

                    foreach (var searchPattern in searchPatterns)
                    {
                        var filePaths = this.FindFiles.EnumerateFiles(directoryPath, searchPattern, searchOption)
                            .Where(x => !exclusionSet.Contains(Path.GetFileName(x)))
                            .ToArray();

                        if (filePaths.Length == 0)
                        {
                            continue;
                        }

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
                                    var assembly = this.assemblyLoader.LoadAssembly(filePath, out _);
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
                                        assemblyErrors.Add(
                                            new ErrorInfo(ErrorCodes.AssemblyLoadFailed, ErrorCodes.AssemblyLoadFailed.Description)
                                            {
                                                Target = filePath,
                                            });
                                        allLoaded = false;
                                    }
                                }
                                else
                                {
                                    allLoaded = false;
                                    assemblyErrors.Add(validationError);
                                }
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
            Console.Error.WriteLine("Loaded {0} in {1}ms", allLoaded ? "all" : "some", watch.ElapsedMilliseconds);
            
            if (directoryErrors.Count > 0)
            {
                Debug.Assert(!allLoaded);
                error = new DiscoveryError(
                            directoryPaths,
                            includeSubdirectories,
                            searchPatterns,
                            exclusionFileNames,
                            exclusionsAreCaseSensitive)
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
                bool exclusionsAreCaseSensitive)
                : base(ErrorCodes.DiscoveryFailed, ErrorCodes.DiscoveryFailed.Description)
            {
                this.DirectoryPaths = directoryPaths?.ToList() ?? new List<string>();
                this.IncludeSubDirectories = includeSubdirectories;
                this.SearchPatterns = searchPatterns?.ToList() ?? new List<string>();
                this.ExclusionFileNames = exclusionFileNames?.ToList() ?? new List<string>();
                this.ExclusionsAreCaseSensitive = exclusionsAreCaseSensitive;
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
                this.ExclusionsAreCaseSensitive = info.GetBoolean(nameof(this.ExclusionsAreCaseSensitive));
            }

            public List<string> DirectoryPaths { get; }

            public bool IncludeSubDirectories { get; }

            public List<string> SearchPatterns { get; }

            public List<string> ExclusionFileNames { get; }

            public bool ExclusionsAreCaseSensitive { get; }

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
                info.AddValue(nameof(this.ExclusionsAreCaseSensitive), this.ExclusionsAreCaseSensitive);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                this.GetObjectData(info, context);
            }

            public override string ToString()
            {
                return new StringBuilder()
                    .AppendLine("DirectoryPaths:")
                    .Append(string.Join(Environment.NewLine + "    ", this.DirectoryPaths)).AppendLine()
                    .AppendFormat("IncludeSubDirectories: {0}", this.IncludeSubDirectories).AppendLine()
                    .AppendLine("SearchPatterns:")
                    .Append(string.Join(Environment.NewLine + "    ", this.SearchPatterns)).AppendLine()
                    .AppendLine("ExclusionFileNames:")
                    .Append(string.Join(Environment.NewLine + "    ", this.ExclusionFileNames)).AppendLine()
                    .AppendFormat("ExclusionsAreCaseSensitive: {0}", this.ExclusionsAreCaseSensitive)
                    .ToString();
            }
        }
    }
}
