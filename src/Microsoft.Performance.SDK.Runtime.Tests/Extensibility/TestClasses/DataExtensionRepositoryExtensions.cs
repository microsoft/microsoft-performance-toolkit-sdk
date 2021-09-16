// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    internal static class DataExtensionRepositoryExtensions
    {
        internal static bool TryAddReference(
            this IDataExtensionRepositoryBuilder self,
            IDataExtensionReference reference)
        {
            switch (reference)
            {
                case ISourceDataCookerReference sdcr:
                    self.AddSourceDataCookerReference(sdcr);
                    return true;

                case ICompositeDataCookerReference cdcr:
                    self.AddCompositeDataCookerReference(cdcr);
                    return true;

                // TODO: __SDK_DP__
                // Redesign Data Processor API
                ////case IDataProcessorReference dpr:
                ////    self.AddDataProcessorReference(dpr);
                ////    return true;

                case ITableExtensionReference ter:
                    self.AddTableExtensionReference(ter);
                    return true;

                default:
                    return false;
            }
        }
    }
}
