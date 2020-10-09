// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    internal interface IDataExtensionDependenciesBuilder
    {
        void AddRequiredSourceDataCookerPath(DataCookerPath dataCookerPath);

        void AddRequiredCompositeDataCookerPath(DataCookerPath dataCookerPath);

        void AddRequiredDataProcessorId(DataProcessorId dataProcessorId);

        void AddRequiredExtensionReferences(IDataExtensionDependencies dependencies);
    }
}
