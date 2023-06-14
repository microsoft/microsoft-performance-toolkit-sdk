// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using ProcessingSourceInfo = Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata.ProcessingSourceInfo;

namespace plugin.tool
{
    internal static class MetadataGenerator
    {
        public static bool TryExtractFromAssembly(string assemblyDir, PluginMetadataInit pluginMetaData)
        {
            using var pluginLoader = new PluginsLoader();
            if (!pluginLoader.TryLoadPlugin(assemblyDir, out ErrorInfo errorInfo))
            {
                Console.WriteLine($"Failed to load plugin: {errorInfo}");
                return false;
            }

            Version? sdkVersion = null;
            var versionChecker = VersionChecker.Create();
            IEnumerable<ProcessingSourceReference> processingSources = pluginLoader.LoadedProcessingSources;
            var processingSourceMetadatas = new List<ProcessingSourceMetadata>();

            foreach (ProcessingSourceReference source in processingSources)
            {
                NuGet.Versioning.SemanticVersion nugetVersion = versionChecker.FindReferencedSdkVersion(source.Type.Assembly);
                var version = new Version(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch);
                sdkVersion = sdkVersion == null ? version : version > sdkVersion ? version : sdkVersion;

                Microsoft.Performance.SDK.Processing.ProcessingSourceInfo sourceInfo = source.Instance.GetAboutInfo();

                IEnumerable<TableMetadata> dataTables = source.Instance.DataTables.Select(x => new TableMetadata(
                    x.Guid,
                    x.Name,
                    x.Description,
                    x.Category,
                    false));

                IEnumerable<TableMetadata> metadataTables = source.Instance.MetadataTables.Select(x => new TableMetadata(
                    x.Guid,
                    x.Name,
                    x.Description,
                    x.Category,
                    true));


                var dsMetadata = new List<DataSourceMetadata>();
                foreach (DataSourceAttribute? ds in source.DataSources)
                {
                    if (ds is FileDataSourceAttribute fds)
                    {
                        dsMetadata.Add(new DataSourceMetadata(fds.FileExtension, fds.Description));
                    }
                    else if (ds is DirectoryDataSourceAttribute)
                    {
                        dsMetadata.Add(new DataSourceMetadata("directory", ds.Description));
                    }
                    else if (ds is ExtensionlessFileDataSourceAttribute)
                    {
                        dsMetadata.Add(new DataSourceMetadata("extensionless", ds.Description));
                    }
                }

                var metadata = new ProcessingSourceMetadata(
                    version: Version.Parse(source.Version),
                    name: source.Name,
                    description: source.Description,
                    guid: source.Instance.TryGetGuid(),
                    aboutInfo: new ProcessingSourceInfo(sourceInfo),
                    availableTables: dataTables.Concat(metadataTables),
                    supportedDataSources: dsMetadata);

                processingSourceMetadatas.Add(metadata);
            }

            pluginMetaData.ProcessingSources = processingSourceMetadatas;
            pluginMetaData.SdkVersion = sdkVersion;

            // TODO: #294 Figure out how to extract Description and ProcessingSourceGuid of a datacooker.
            pluginMetaData.DataCookers = pluginLoader.Extensions.SourceDataCookers.Concat(pluginLoader.Extensions.CompositeDataCookers)
                .Select(x => new DataCookerMetadata(x.DataCookerId, string.Empty, string.Empty)).ToList();

            pluginMetaData.ExtensibleTables = pluginLoader.Extensions.TablesById.Values
                .Select(x => new TableMetadata(
                    x.TableDescriptor.Guid,
                    x.TableDescriptor.Name,
                    x.TableDescriptor.Description,
                    x.TableDescriptor.Category,
                    x.TableDescriptor.IsMetadataTable)).ToList();

            return true;
        }
    }
}
