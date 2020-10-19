// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository
{
    /// <summary>
    ///     Provides a way to add data extensions to a data extension repository.
    /// </summary>
    public interface IDataExtensionRepositoryBuilder
        : IDataExtensionRepository
    {
        /// <summary>
        ///     Add a source data cooker.
        /// </summary>
        /// <param name="dataCooker">
        ///     Source data cooker reference.
        /// </param>
        /// <returns>
        ///     true if success.
        /// </returns>
        bool AddSourceDataCookerReference(ISourceDataCookerReference dataCooker);

        /// <summary>
        ///     Add a composite data cooker.
        /// </summary>
        /// <param name="dataCooker">
        ///     Composite data cooker reference.
        /// </param>
        /// <returns>
        ///     true if success.
        /// </returns>
        bool AddCompositeDataCookerReference(ICompositeDataCookerReference dataCooker);

        /// <summary>
        ///     Add a table data extension.
        /// </summary>
        /// <param name="tableExtensionReference">
        ///     Table extension reference.
        /// </param>
        /// <returns>
        ///     true if success.
        /// </returns>
        bool AddTableExtensionReference(ITableExtensionReference tableExtensionReference);

        /// <summary>
        ///     Add a data processor extension.
        /// </summary>
        /// <param name="dataProcessorReference">
        ///     Data processor reference.
        /// </param>
        /// <returns>
        ///     true if success.
        /// </returns>
        bool AddDataProcessorReference(IDataProcessorReference dataProcessorReference);

        /// <summary>
        ///     Called after all data extensions have been added to the repository to perform additional processing.
        /// </summary>
        void FinalizeDataExtensions();
    }
}
