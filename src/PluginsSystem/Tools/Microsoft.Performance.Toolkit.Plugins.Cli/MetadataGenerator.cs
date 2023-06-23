// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Diagnostics;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using ProcessingSourceInfo = Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata.ProcessingSourceInfo;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal class MetadataGenerator
        : IMetadataGenerator
    {
        private readonly ILogger logger;

        public MetadataGenerator(Func<Type, ILogger> loggerFactory)
        {
            this.logger = loggerFactory(typeof(MetadataGenerator));
        }


        public bool TryCreateMetadata(string assemblyDir, out PluginMetadataInit pluginMetadata)
        {
            pluginMetadata = new();

            using var pluginLoader = new PluginsLoader();
            if (!pluginLoader.TryLoadPlugin(assemblyDir, out ErrorInfo errorInfo))
            {
                this.logger.Error($"Failed to load plugin: {errorInfo}");
                return false;
            }

            Version sdkVersion = null;
            var versionChecker = VersionChecker.Create();
            var processingSourcesMetadata = new List<ProcessingSourceMetadata>();
            foreach (ProcessingSourceReference source in pluginLoader.LoadedProcessingSources)
            {
                // Version (use the latest version if different across processing sources)
                Version version = GetSDKVersion(source, versionChecker);
                sdkVersion = sdkVersion == null ? version : version > sdkVersion ? version : sdkVersion;

                try
                {
                    ProcessingSourceMetadata metadata = CreateProcessingSourceMetadata(source);

                    processingSourcesMetadata.Add(metadata);
                }
                catch (Exception e)
                {
                    this.logger.Error($"Failed to create metadata for processing source {source.Type.Name}: {e}");
                    return false;
                }
            }

            try
            {
                pluginMetadata.InstalledSize = (ulong)CalculateFolderSize(assemblyDir);
            }
            catch (Exception e)
            {
                this.logger.Error($"Failed to calculate the installed size of this plugin: {e}");
                return false;
            }

            pluginMetadata.ProcessingSources = processingSourcesMetadata;
            pluginMetadata.SdkVersion = sdkVersion;

            // TODO: #294 Figure out how to extract description of a datacooker.
            pluginMetadata.DataCookers = pluginLoader.Extensions.SourceDataCookers.Concat(pluginLoader.Extensions.CompositeDataCookers)
                .Select(x => new DataCookerMetadata(x.DataCookerId, null, x.SourceParserId)).ToList();

            pluginMetadata.ExtensibleTables = pluginLoader.Extensions.TablesById.Values
                .Select(x => new TableMetadata(
                    x.TableDescriptor.Guid,
                    x.TableDescriptor.Name,
                    x.TableDescriptor.Description,
                    x.TableDescriptor.Category,
                    x.TableDescriptor.IsMetadataTable)).ToList();

            return true;
        }

        private static long CalculateFolderSize(string folderPath)
        {
            long totalSize = 0;

            if (!Directory.Exists(folderPath))
            {
                return totalSize;
            }

            foreach (string file in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                totalSize += new FileInfo(file).Length;
            }

            return totalSize;
        }

        private static Version GetSDKVersion(ProcessingSourceReference psr, VersionChecker versionChecker)
        {
            NuGet.Versioning.SemanticVersion nugetVersion = versionChecker.FindReferencedSdkVersion(psr.Type.Assembly);
            var version = new Version(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch);
            return version;
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
                    dataSourcesMetadata.Add(new DataSourceMetadata(Constants.DirectoryDataSourceName, ds.Description));
                }
                else if (ds is ExtensionlessFileDataSourceAttribute)
                {
                    dataSourcesMetadata.Add(new DataSourceMetadata(Constants.ExtensionlessDataSourceName, ds.Description));
                }
                else
                {
                    Debug.Assert(false, "Unknown DataSourceAttribute type");
                }
            }

            var metadata = new ProcessingSourceMetadata(
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
