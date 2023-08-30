// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using ProcessingSourceInfo = Microsoft.Performance.Toolkit.Plugins.Core.Metadata.ProcessingSourceInfo;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Processing
{
    /// <summary>
    ///     Processes a plugin's artifacts.
    /// </summary>
    internal class PluginArtifactsProcessor
        : IPluginArtifactsProcessor
    {
        private readonly IManifestFileValidator manifestValidator;
        private readonly IManifestFileReader manifestReader;
        private readonly ILogger<PluginArtifactsProcessor> logger;

        public PluginArtifactsProcessor(
            IManifestFileValidator manifestValidator,
            IManifestFileReader manifestReader,
            ILogger<PluginArtifactsProcessor> logger)
        {
            this.manifestValidator = manifestValidator;
            this.manifestReader = manifestReader;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool TryProcess(PluginArtifacts artifacts, [NotNullWhen(true)] out ProcessedPluginResult? processedPlugin)
        {
            processedPlugin = null;            
            if (!TryProcessSourceDir(artifacts.SourceDirectoryFullPath, out ProcessedPluginSourceDirectory? processedDir))
            {
                this.logger.LogError($"Failed to process source directory {artifacts.SourceDirectoryFullPath}.");
                return false;
            }

            string manifestFilePath = artifacts.ManifestFileFullPath;

            try
            {
                if (!this.manifestValidator.IsValid(manifestFilePath, out List<string> validationMessages))
                {
                    string errors = string.Join(Environment.NewLine, validationMessages);
                    this.logger.LogWarning($"Manifest file failed some json schema format validation checks: \n{errors}");
                    this.logger.LogWarning("Continuing with packing process but it is recommended to fix the validation errors and repack before publishing the plugin.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to validate manifest file {manifestFilePath} due to an exception {ex.Message}.");
                return false;
            }

            if (!this.manifestReader.TryRead(manifestFilePath, out PluginManifest? manifest))
            {
                this.logger.LogError($"Failed to read manifest file {manifestFilePath}.");
                return false;
            }
            
            PluginMetadata metadata = GenerateMetadata(processedDir, manifest);
            
            if (!TryGenerateContentsMetadata(processedDir, out PluginContentsMetadata? contentsMetadata))
            {
                this.logger.LogError($"Failed to generate contents metadata for plugin.");
                return false;
            }

            processedPlugin = new ProcessedPluginResult(artifacts.SourceDirectoryFullPath, processedDir.AllContentFilePaths, metadata, contentsMetadata!);
            return true;
        }

        private bool TryProcessSourceDir(
           string sourceDir,
           [NotNullWhen(true)] out ProcessedPluginSourceDirectory? processedDir)
        {
            processedDir = null;
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
                        this.logger.LogError($"{Constants.SdkAssemblyName} should not present in the directory.");
                        return false;
                    }

                    Assembly assembly;
                    try
                    {
                        assembly = Assembly.LoadFrom(file);
                    }
                    catch (BadImageFormatException)
                    {
                        // TODO: Add support for excluding certain files from being loaded.
                        // Skipping this for now.
                        continue;
                    }
                    catch (FileLoadException ex)
                    {
                        this.logger.LogError($"Unable to load file {fileName}: {ex.Message}");
                        return false;
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
                            this.logger.LogError(
                                $"Mutiple versions of {Constants.SdkAssemblyName} are referenced in the plugin: {sdkVersion} and {curVersion}. Only one version is allowed.");
                            return false;
                        }
                    }
                }
                else if (fileName.Equals(Constants.BundledManifestName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                filesToPack.Add(Path.GetRelativePath(sourceDir, file));
                totalSize += new FileInfo(file).Length;
            }

            if (dllCount == 0)
            {
                this.logger.LogError($"Directory does not contain any DLLs: {sourceDir}.");
                return false;
            }

            if (sdkVersion == null)
            {
                this.logger.LogError($"Invalid plugin: {Constants.SdkAssemblyName} is not referenced anywhere in the plugin.");
                return false;
            }

            processedDir = new ProcessedPluginSourceDirectory(
                sourceDir,
                filesToPack,
                manifestFilePath,
                sdkVersion,
                totalSize);
            
            return true;
        }

        private PluginMetadata GenerateMetadata(ProcessedPluginSourceDirectory processedDir, PluginManifest manifest)
        {
            this.logger.LogTrace($"Generating metadata for plugin {manifest.Identity.Id}-{manifest.Identity.Version}");

            PluginIdentity identity = new(manifest.Identity.Id, manifest.Identity.Version);
            ulong installedSize = (ulong)processedDir.PluginSize;
            IEnumerable<PluginOwnerInfo> owners = manifest.Owners.Select(
                o => new PluginOwnerInfo(o.Name, o.Address, o.EmailAddresses.ToArray(), o.PhoneNumbers.ToArray()));
            Version sdkVersion = processedDir.SdkVersion;

            PluginMetadata metadata = new(
                identity,
                installedSize,
                manifest.DisplayName,
                manifest.Description,
                sdkVersion,
                manifest.ProjectUrl, owners);

            return metadata;
        }

        private bool TryGenerateContentsMetadata(ProcessedPluginSourceDirectory processedDir, out PluginContentsMetadata? contentsMetadata)
        {
            contentsMetadata = null;
            string sourceDir = processedDir.FullPath;
            using PluginsLoader pluginsLoader = new();

            if (!pluginsLoader.TryLoadPlugin(sourceDir, out ErrorInfo errorInfo))
            {
                // TODO: Check error codes and throw more specific exceptions
                this.logger.LogError($"Failed to load plugin from {sourceDir}: {errorInfo.Message}");
                return false;
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
            
            contentsMetadata = new(processingSourcesMetadata, dataCookers, tables);
            return true;
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

        private record ProcessedPluginSourceDirectory(
            string FullPath,
            IReadOnlyList<string> AllContentFilePaths,
            string? ManifestFilePath,
            Version SdkVersion,
            long PluginSize);
    }
}
