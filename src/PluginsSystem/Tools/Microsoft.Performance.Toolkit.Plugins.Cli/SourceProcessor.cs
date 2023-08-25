// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using ProcessingSourceInfo = Microsoft.Performance.Toolkit.Plugins.Core.Metadata.ProcessingSourceInfo;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal class SourceProcessor
        : IPluginContentsProcessor
    {
        private readonly IManifestFileValidator manifestValidator;
        private readonly IManifestFileReader manifestReader;
        private readonly ILogger<SourceProcessor> logger;
        
        public SourceProcessor(
            IManifestFileValidator manifestValidator,
            IManifestFileReader manifestReader,
            ILogger<SourceProcessor> logger)
        {
            this.manifestValidator = manifestValidator;
            this.manifestReader = manifestReader;
            this.logger = logger;
        }

        public ProcessedPluginContents Process(TransformedBase options)
        {
            bool shouldInclude = options.ManifestFileFullPath == null;
            ScannedResult processedDir = ProcessDir(options.SourceDirectoryFullPath, shouldInclude);

            string? manifestFilePath = shouldInclude ? processedDir.ManifestFilePath : options.ManifestFileFullPath;
            if (!this.manifestValidator.IsValid(manifestFilePath!, out List<string> validationMessages))
            {
                string errors = string.Join(Environment.NewLine, validationMessages);
                this.logger.LogWarning($"Manifest file failed some json schema format validation checks: \n{errors}");
                this.logger.LogWarning("Continuing with packing process but it is recommended to fix the validation errors and repack before publishing the plugin.");
            }

            PluginManifest manifest = this.manifestReader.Read(manifestFilePath!);
            PluginMetadata metadata = GenerateMetadata(processedDir, manifest);
            PluginContentsMetadata contentsMetadata = GenerateContentsMetadata(processedDir);

            return new ProcessedPluginContents(options.SourceDirectoryFullPath,processedDir.AllContentFilePaths, metadata, contentsMetadata);
        }

        private record ScannedResult(
            string FullPath,
            IReadOnlyList<string> AllContentFilePaths,
            string? ManifestFilePath,
            Version SdkVersion,
            long PluginSize);

        private ScannedResult ProcessDir(
           string sourceDir,
           bool manifestShouldPresent)
        {
            string? manifestFilePath = null;
            Version? sdkVersion = null;
            long totalSize = 0;
            int dllCount = 0;
            var filesToPack = new List<string>();

            foreach (string file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    dllCount++;
                    if (Path.GetFileNameWithoutExtension(file).Equals(Constants.SdkAssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidPluginContentException($"{Constants.SdkAssemblyName} should not present in the directory.");
                    }

                    Assembly assembly;
                    try
                    {
                        assembly = Assembly.LoadFrom(file);
                    }
                    catch (BadImageFormatException ex)
                    {
                        throw new InvalidPluginContentException($"File not readable: {fileName}", ex);
                    }
                    catch (FileLoadException ex)
                    {
                        throw new ConsoleRuntimeException($"Unable to load file: {fileName}", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new ConsoleRuntimeException($"Unexpected Error occurs when loading {fileName}: {ex.Message}", ex);
                    }

                    AssemblyName? sdkRef = assembly.GetReferencedAssemblies()
                        .FirstOrDefault(assemblyName => assemblyName.Name?.Equals(Constants.SdkAssemblyName, StringComparison.OrdinalIgnoreCase) == true);

                    // Check if the assembly references the target dependency
                    if (sdkRef != null)
                    {
                        Version? curVersion = sdkRef.Version;
                        if (sdkVersion == null)
                        {
                            sdkVersion = curVersion;
                        }
                        else if (sdkVersion != curVersion)
                        {
                            throw new InvalidPluginContentException(
                                $"Mutiple versions of {Constants.SdkAssemblyName} are referenced in the plugin: {sdkVersion} and {curVersion}. Only one version is allowed.");
                        }
                    }

                }
                else if (fileName.Equals(Constants.BundledManifestName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!manifestShouldPresent)
                    {
                        this.logger.LogWarning($"Directory contains {Constants.BundledManifestName} when it should not: {sourceDir}. This file will be ignored.");
                    }
                    else if (manifestFilePath != null)
                    {
                        throw new InvalidPluginContentException($"Directory contains multiple manifests: {manifestFilePath}, {file}. Only one manifest is allowed.");
                    }

                    manifestFilePath = file;
                    continue;
                }

                filesToPack.Add(Path.GetRelativePath(sourceDir, file));
                totalSize += new FileInfo(file).Length;
            }

            if (dllCount == 0)
            {
                throw new InvalidPluginContentException($"Directory does not contain any DLLs: {sourceDir}.");
            }

            // Check if the directory contains the manifest file if it should
            if (manifestShouldPresent && manifestFilePath == null)
            {
                throw new InvalidPluginContentException($"Directory does not contain {Constants.BundledManifestName} as expected: {sourceDir}.");
            }

            if (sdkVersion == null)
            {
                throw new InvalidPluginContentException($"Invalid plugin: {Constants.SdkAssemblyName} is not referenced anywhere in the plugin.");
            }
            
            return new ScannedResult(
                sourceDir,
                filesToPack,
                manifestFilePath,
                sdkVersion,
                totalSize);
        }

        private PluginMetadata GenerateMetadata(ScannedResult scannedDir, PluginManifest manifest)
        {
            PluginIdentity identity = new(manifest.Identity.Id, manifest.Identity.Version);
            ulong installedSize = (ulong)scannedDir.PluginSize;
            IEnumerable<PluginOwnerInfo> owners = manifest.Owners.Select(
                o => new PluginOwnerInfo(o.Name, o.Address, o.EmailAddresses.ToArray(), o.PhoneNumbers.ToArray()));
            Version sdkVersion = scannedDir.SdkVersion;

            PluginMetadata metadata = new(
                identity,
                installedSize,
                manifest.DisplayName,
                manifest.Description,
                sdkVersion,
                manifest.ProjectUrl, owners);

            return metadata;
        }

        private PluginContentsMetadata GenerateContentsMetadata(ScannedResult scannedDir)
        {
            string sourceDir = scannedDir.FullPath;
            using PluginsLoader pluginsLoader = new();
            
            if (!pluginsLoader.TryLoadPlugin(sourceDir, out ErrorInfo errorInfo))
            {
                this.logger.LogDebug($"Failed to load plugin from {sourceDir}: {errorInfo}");

                // TODO: Check error codes and throw more specific exceptions
                throw new InvalidPluginContentException($"Failed to load plugin from {sourceDir}: {errorInfo.Message}");
            }
            
            var processingSourcesMetadata = pluginsLoader.LoadedProcessingSources.Select(x => CreateProcessingSourceMetadata(x)).ToList();

            // TODO: #294 Figure out how to extract description of a datacooker.
            var dataCookers = pluginsLoader.Extensions.SourceDataCookers
                .Concat(pluginsLoader.Extensions.CompositeDataCookers)
                .Select(x => new DataCookerMetadata(x.DataCookerId, null, x.SourceParserId))
                .ToList();

            var tables = pluginsLoader.Extensions.TablesById.Values
                .Select(x => new TableMetadata(
                    x.TableDescriptor.Guid,
                    x.TableDescriptor.Name,
                    x.TableDescriptor.Description,
                    x.TableDescriptor.Category,
                    x.TableDescriptor.IsMetadataTable))
                .ToList();

            PluginContentsMetadata contentsMetadata = new(processingSourcesMetadata, dataCookers, tables);
            return contentsMetadata;
        }

        private static ProcessingSourceMetadata CreateProcessingSourceMetadata(ProcessingSourceReference psr)
        {
            // Tables
            IEnumerable<TableMetadata> dataTables = psr.Instance.DataTables.Select(x => new TableMetadata(
                x.Guid,
                x.Name,
                x.Description,
                x.Category,
                false));

            IEnumerable<TableMetadata> metadataTables = psr.Instance.MetadataTables.Select(x => new TableMetadata(
                x.Guid,
                x.Name,
                x.Description,
                x.Category,
                true));

            // Data Sources
            var dataSourcesMetadata = new List<DataSourceMetadata>();
            foreach (DataSourceAttribute? ds in psr.DataSources)
            {
                if (ds is FileDataSourceAttribute fds)
                {
                    dataSourcesMetadata.Add(new DataSourceMetadata(fds.FileExtension, fds.Description));
                }
                else if (ds is DirectoryDataSourceAttribute)
                {
                    dataSourcesMetadata.Add(new DataSourceMetadata(DataSourceNameConstants.DirectoryDataSourceName, ds.Description));
                }
                else if (ds is ExtensionlessFileDataSourceAttribute)
                {
                    dataSourcesMetadata.Add(new DataSourceMetadata(DataSourceNameConstants.ExtensionlessDataSourceName, ds.Description));
                }
                else
                {
                    Debug.Assert(false, $"Unknown DataSourceAttribute type: {ds.GetType()}");
                }
            }

            ProcessingSourceMetadata metadata = new(
                version: Version.Parse(psr.Version),
                name: psr.Name,
                description: psr.Description,
                guid: psr.Instance.TryGetGuid(),
                aboutInfo: new ProcessingSourceInfo(psr.Instance.GetAboutInfo()),
                availableTables: dataTables.Concat(metadataTables),
                supportedDataSources: dataSourcesMetadata);

            return metadata;
        }
    }
}
