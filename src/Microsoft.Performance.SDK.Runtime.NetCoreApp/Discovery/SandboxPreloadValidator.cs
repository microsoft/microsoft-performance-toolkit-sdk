﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Performance.SDK.Runtime.Discovery;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery
{
    public sealed class SandboxPreloadValidator
        : IPreloadValidator
    {
        private readonly MetadataAssemblyResolver resolver;
        private readonly VersionChecker versionChecker;
        private readonly ConcurrentDictionary<string, MetadataLoadContext> loadContextsByDirectory;

        private bool isDisposed;

        public SandboxPreloadValidator(
            IEnumerable<string> assemblyPaths,
            VersionChecker versionChecker)
        {
            //
            // https://docs.microsoft.com/en-us/dotnet/standard/assembly/inspect-contents-using-metadataloadcontext
            //

            var runtime = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var allDlls = assemblyPaths.Concat(runtime);

            this.resolver = new PathAssemblyResolver(allDlls);
            this.versionChecker = versionChecker;
            this.loadContextsByDirectory = new ConcurrentDictionary<string, MetadataLoadContext>();
        }

        public bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error)
        {
            var loadContext = this.loadContextsByDirectory.GetOrAdd(
                Path.GetDirectoryName(fullPath),
                _ => this.CreateLoadContext());

            Assembly loaded;
            try
            {
                loaded = loadContext.LoadFromAssemblyPath(fullPath);
            }
            catch (FileNotFoundException e)
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    ErrorCodes.AssemblyLoadFailed.Description)
                {
                    Target = fullPath,
                    Details = new[]
                    {
                            new ErrorInfo(ErrorCodes.FileNotFound, e.Message)
                            {
                                Target = fullPath,
                            },
                        },
                };

                return false;
            }
            catch (FileLoadException e)
            {
                error = new ErrorInfo(
                    ErrorCodes.AssemblyLoadFailed,
                    ErrorCodes.AssemblyLoadFailed.Description)
                {
                    Target = fullPath,
                    Details = new[]
                    {
                            new ErrorInfo(ErrorCodes.FileLoadFailure, ErrorCodes.FileLoadFailure.Description)
                            {
                                Target = fullPath,
                                Details = new[]
                                {
                                    new ErrorInfo(ErrorCodes.FileLoadFailure, e.Message)
                                    {
                                        Target = fullPath,
                                    },
                                },
                            },
                        },
                };

                return false;
            }
            catch (Exception e)
            {
                error = new ErrorInfo(
                    ErrorCodes.Unexpected,
                    ErrorCodes.Unexpected.Description)
                {
                    Target = fullPath,
                    Details = new[]
                    {
                            new ErrorInfo(ErrorCodes.Unexpected, e.Message)
                            {
                                Target = fullPath,
                            },
                        },
                };

                return false;
            }

            Debug.Assert(loaded != null);

            if (!this.IsSdkValid(loaded, out error))
            {
                return false;
            }

            //
            // Add any other checks here
            //

            error = ErrorInfo.None;
            return true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private MetadataLoadContext CreateLoadContext()
        {
            return new MetadataLoadContext(this.resolver);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var kvp in this.loadContextsByDirectory)
                {
                    kvp.Value.Dispose();
                }

                this.loadContextsByDirectory.Clear();
            }

            this.isDisposed = true;
        }

        private bool IsSdkValid(Assembly assembly, out ErrorInfo error)
        {
            Debug.Assert(assembly != null);

            var sdkReference = this.versionChecker.FindReferencedSdkVersion(assembly);
            if (sdkReference != null &&
                !this.versionChecker.IsVersionSupported(sdkReference))
            {
                error = new SdkMismatchError(sdkReference, this.versionChecker.Sdk)
                {
                    Target = assembly.Location,
                };

                return false;
            }

            error = ErrorInfo.None;
            return true;
        }

        private class SdkMismatchError
            : ErrorInfo
        {
            public SdkMismatchError(
                SemanticVersion referencedSdkVersion,
                SemanticVersion hostedSdkVersion)
                : base(ErrorCodes.SdkVersionIncompatible, ErrorCodes.SdkVersionIncompatible.Description)
            {
                this.ReferencedSdkVersion = referencedSdkVersion;
                this.HostedSdkVersion = hostedSdkVersion;
            }

            public SemanticVersion ReferencedSdkVersion { get; }

            public SemanticVersion HostedSdkVersion { get; }
        }
    }
}
