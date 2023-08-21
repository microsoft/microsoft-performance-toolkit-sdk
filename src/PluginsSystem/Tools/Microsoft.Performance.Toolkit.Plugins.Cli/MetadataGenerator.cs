// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Diagnostics;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using ProcessingSourceInfo = Microsoft.Performance.Toolkit.Plugins.Core.Metadata.ProcessingSourceInfo;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class MetadataGenerator
        : IMetadataGenerator
    {
        private readonly ILogger logger;

        public MetadataGenerator(ILogger<MetadataGenerator> logger)
        {
            this.logger = logger;
        }


        public AllMetadata? TryGen(ValidatedPluginDirectory pluginDirectory, PluginManifest manifest)
        {
            string assemblyDir = pluginDirectory.FullPath;
            
            using var pluginLoader = new PluginsLoader();
            if (!pluginLoader.TryLoadPlugin(assemblyDir, out ErrorInfo errorInfo))
            {
                this.logger.LogError($"Failed to load plugin: {errorInfo}");
                return null;
            }

            var processingSourcesMetadata = pluginLoader.LoadedProcessingSources.Select(x => CreateProcessingSourceMetadata(x)).ToList();


            // TODO: #294 Figure out how to extract description of a datacooker.
            var dataCookers = pluginLoader.Extensions.SourceDataCookers
                .Concat(pluginLoader.Extensions.CompositeDataCookers)
                .Select(x => new DataCookerMetadata(x.DataCookerId, null, x.SourceParserId))
                .ToList();

            var tables = pluginLoader.Extensions.TablesById.Values
                .Select(x => new TableMetadata(
                    x.TableDescriptor.Guid,
                    x.TableDescriptor.Name,
                    x.TableDescriptor.Description,
                    x.TableDescriptor.Category,
                    x.TableDescriptor.IsMetadataTable))
                .ToList();


            PluginIdentity identity = new(manifest.Identity.Id, manifest.Identity.Version);
            ulong installedSize = (ulong)pluginDirectory.PluginSize;
            IEnumerable<PluginOwnerInfo> owners = manifest.Owners.Select(o => new PluginOwnerInfo(o.Name, o.Address, o.EmailAddresses.ToArray(), o.PhoneNumbers.ToArray()));
            Version sdkVersion = pluginDirectory.SdkVersion;

            PluginMetadata metadata = new(identity, installedSize, manifest.DisplayName, manifest.Description, sdkVersion, manifest.ProjectUrl, owners);
            PluginContentsMetadata contentsMetadata = new(processingSourcesMetadata, dataCookers, tables);

            return new AllMetadata(metadata, contentsMetadata);
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
