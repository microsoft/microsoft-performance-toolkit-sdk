using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class ExtractedMetadata
    {
        public ulong InstalledSize { get; init; }

        public Version SdkVersion { get; init; }

        public IEnumerable<ProcessingSourceMetadata> ProcessingSources { get; init; }

        public IEnumerable<DataCookerMetadata> DataCookers { get; init; }

        public IEnumerable<TableMetadata> ExtensibleTables { get; init; }
    }
}
