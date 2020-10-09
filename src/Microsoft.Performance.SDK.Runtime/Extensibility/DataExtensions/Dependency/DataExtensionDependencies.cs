// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    internal class DataExtensionDependencies
        : IDataExtensionDependencies,
          IDataExtensionDependenciesBuilder
    {
        private readonly HashSet<DataCookerPath> requiredSourceDataCookerPaths;
        private readonly HashSet<DataCookerPath> requiredCompositeDataCookerPaths;
        private readonly HashSet<DataProcessorId> requiredDataProcessorIds;

        public DataExtensionDependencies()
        {
            this.requiredSourceDataCookerPaths = new HashSet<DataCookerPath>();
            this.requiredCompositeDataCookerPaths = new HashSet<DataCookerPath>();
            this.requiredDataProcessorIds = new HashSet<DataProcessorId>();
        }

        public DataExtensionDependencies(DataExtensionDependencies other)
        {
            Guard.NotNull(other, nameof(other));

            this.requiredSourceDataCookerPaths = new HashSet<DataCookerPath>(other.requiredSourceDataCookerPaths);
            this.requiredCompositeDataCookerPaths = new HashSet<DataCookerPath>(other.requiredCompositeDataCookerPaths);
            this.requiredDataProcessorIds = new HashSet<DataProcessorId>(other.requiredDataProcessorIds);
        }

        public IReadOnlyCollection<DataCookerPath> RequiredSourceDataCookerPaths => this.requiredSourceDataCookerPaths;

        public IReadOnlyCollection<DataCookerPath> RequiredCompositeDataCookerPaths => this.requiredCompositeDataCookerPaths;

        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessorIds => this.requiredDataProcessorIds;

        public void AddRequiredSourceDataCookerPath(DataCookerPath dataCookerPath)
        {
            this.requiredSourceDataCookerPaths.Add(dataCookerPath);
        }

        public void AddRequiredCompositeDataCookerPath(DataCookerPath dataCookerPath)
        {
            this.requiredCompositeDataCookerPaths.Add(dataCookerPath);
        }

        public void AddRequiredDataProcessorId(DataProcessorId dataProcessorId)
        {
            this.requiredDataProcessorIds.Add(dataProcessorId);
        }

        public void AddRequiredExtensionReferences(IDataExtensionDependencies dependencies)
        {
            if (dependencies is null)
            {
                // nothing to add
                return;
            }

            foreach (var dataCookerPath in dependencies.RequiredSourceDataCookerPaths)
            {
                this.AddRequiredSourceDataCookerPath(dataCookerPath);
            }

            foreach (var dataCookerPath in dependencies.RequiredCompositeDataCookerPaths)
            {
                this.AddRequiredCompositeDataCookerPath(dataCookerPath);
            }

            foreach (var processorId in dependencies.RequiredDataProcessorIds)
            {
                this.AddRequiredDataProcessorId(processorId);
            }
        }
    }
}
