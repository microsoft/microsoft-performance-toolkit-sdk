// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     This class is used to retrieve data from the subset of all available data extension objects. It is used to
    ///     limit the data available to a given data extension, based on the requirements declared by that data extension.
    /// </summary>
    internal class FilteredDataRetrieval
        : IDataExtensionRetrieval
    {
        private readonly ICookedDataRetrieval sourceCookerData;
        private readonly ICompositeCookerRepository compositeCookers;
        private readonly IDataExtensionRepository dataExtensionRepository;
        private readonly Func<DataCookerPath, IDataExtensionRetrieval> createCompositeDataRetrieval;
        private readonly Func<DataProcessorId, IDataExtensionRetrieval> createProcessorDataRetrieval;
        private readonly IDataExtensionDependencies extensionDependencies;

        /// <summary>
        ///     Creates an instance of the <see cref="FilteredDataRetrieval"/> class for a given data extension.
        /// </summary>
        /// <param name="cookerData">
        ///     Provides access to source cooker data and composite cookers.
        /// </param>
        /// <param name="dataExtensionRepository">
        ///     Provides access to data processors (to be removed for V1).
        /// </param>
        /// <param name="createProcessorDataRetrieval">
        ///     Creates a data retrieval instance for a data processor extension.
        /// </param>
        /// <param name="extensionDependencies">
        ///     The set of dependencies that the target data extension for this instance has access.
        /// </param>
        internal FilteredDataRetrieval(
            ICookedDataRetrieval sourceCookerData,
            ICompositeCookerRepository compositeCookers,
            IDataExtensionRepository dataExtensionRepository,
            Func<DataCookerPath, IDataExtensionRetrieval> createCompositeDataRetrieval,
            Func<DataProcessorId, IDataExtensionRetrieval> createProcessorDataRetrieval,
            IDataExtensionDependencies extensionDependencies)
        {
            Debug.Assert(sourceCookerData != null, nameof(sourceCookerData));
            Debug.Assert(compositeCookers != null, nameof(compositeCookers));
            Debug.Assert(dataExtensionRepository != null, nameof(dataExtensionRepository));
            Debug.Assert(extensionDependencies != null, nameof(extensionDependencies));
            Debug.Assert(createCompositeDataRetrieval != null, nameof(createCompositeDataRetrieval));
            Debug.Assert(createProcessorDataRetrieval != null, nameof(createProcessorDataRetrieval));

            this.sourceCookerData = sourceCookerData;
            this.compositeCookers = compositeCookers;
            this.dataExtensionRepository = dataExtensionRepository;
            this.createCompositeDataRetrieval = createCompositeDataRetrieval;
            this.createProcessorDataRetrieval = createProcessorDataRetrieval;
            this.extensionDependencies = extensionDependencies;
        }

        /// <inheritdoc/>
        public T QueryOutput<T>(DataOutputPath dataOutputPath)
        {
            if (dataOutputPath.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
            {
                // this is a source cooker, so it already exists
                return this.QuerySourceOutput<T>(dataOutputPath);
            }
            else
            {
                return this.QueryCompositeOutput<T>(dataOutputPath);
            }
        }

        /// <inheritdoc/>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            if (dataOutputPath.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
            {
                // this is a source cooker, so it already exists
                return this.QuerySourceOutput(dataOutputPath);
            }
            else
            {
                return this.QueryCompositeOutput(dataOutputPath);
            }
        }

        /// <inheritdoc/>
        public bool TryQueryOutput<T>(DataOutputPath dataOutputPath, out T result)
        {
            if (dataOutputPath.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
            {
                // this is a source cooker, so it already exists
                return this.TryQuerySourceOutput<T>(dataOutputPath, out result);
            }
            else
            {
                return this.TryQueryCompositeOutput<T>(dataOutputPath, out result);
            }
        }

        /// <inheritdoc/>
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (dataOutputPath.CookerPath.DataCookerType == DataCookerType.SourceDataCooker)
            {
                // this is a source cooker, so it already exists
                return this.TryQuerySourceOutput(dataOutputPath, out result);
            }
            else
            {
                return this.TryQueryCompositeOutput(dataOutputPath, out result);
            }
        }

        /// <inheritdoc/>
        public object QueryDataProcessor(DataProcessorId dataProcessorId)
        {
            Guard.NotNull(dataProcessorId, nameof(dataProcessorId));

            if (!this.extensionDependencies.RequiredDataProcessorIds.Contains(dataProcessorId))
            {
                throw new ArgumentException(
                    $"The requested data processor is not available: {dataProcessorId}. " +
                    "Consider adding it to the requirements for this data extension.",
                    nameof(dataProcessorId));
            }

            var processorReference = this.dataExtensionRepository.GetDataProcessorReference(dataProcessorId);
            if (processorReference == null)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve a reference to data processor: {dataProcessorId}.");
            }

            var processorDataRetrieval =
                this.createProcessorDataRetrieval(dataProcessorId);

            return processorReference.GetOrCreateInstance(processorDataRetrieval);
        }

        private void ValidateSourceDataCooker(DataOutputPath dataOutputPath)
        {
            var cookerPath = dataOutputPath.CookerPath;
            if (!this.extensionDependencies.RequiredSourceDataCookerPaths.Contains(cookerPath))
            {
                throw new ArgumentException(
                    $"The requested data cooker is not available: {dataOutputPath}. " +
                    "Consider adding it to the requirements for this data extension.",
                    nameof(dataOutputPath));
            }
        }

        private bool TryValidateSourceDataCooker(DataOutputPath dataOutputPath)
        {
            return this.extensionDependencies.RequiredSourceDataCookerPaths.Contains(dataOutputPath.CookerPath);
        }

        private void ValidateCompositeDataCooker(DataOutputPath dataOutputPath)
        {
            var cookerPath = dataOutputPath.CookerPath;

            if (!this.extensionDependencies.RequiredCompositeDataCookerPaths.Contains(cookerPath))
            {
                throw new ArgumentException(
                    $"The requested data cooker is not available: {dataOutputPath}. " +
                    "Consider adding it to the requirements for this data extension.",
                    nameof(dataOutputPath));
            }
        }

        /// <summary>
        ///     Check that the requested composite cooker is available to this filtered data set.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Path to a composite data cooker.
        /// </param>
        /// <param name="compositeCooker">
        ///     An instance of the composite data cooker.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the composite cooker is valid; <c>false</c> otherwise.
        /// </returns>
        private bool TryValidateCompositeDataCooker(DataOutputPath dataOutputPath/*, out IDataCooker compositeCooker*/)
        {
            var cookerPath = dataOutputPath.CookerPath;

            if (!this.extensionDependencies.RequiredCompositeDataCookerPaths.Contains(cookerPath))
            {
                return false;
            }

            return true;
        }

        private T QuerySourceOutput<T>(DataOutputPath dataOutputPath)
        {
            this.ValidateSourceDataCooker(dataOutputPath);
            return this.sourceCookerData.QueryOutput<T>(dataOutputPath);
        }

        private bool TryQuerySourceOutput<T>(DataOutputPath dataOutputPath, out T result)
        {
            if (this.TryValidateSourceDataCooker(dataOutputPath))
            {
                return this.sourceCookerData.TryQueryOutput<T>(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private T QueryCompositeOutput<T>(DataOutputPath dataOutputPath)
        {
            this.ValidateCompositeDataCooker(dataOutputPath);

            return GetCompositeDataRetrieval(dataOutputPath).QueryOutput<T>(dataOutputPath);
        }

        private bool TryQueryCompositeOutput<T>(DataOutputPath dataOutputPath, out T result)
        {
            if (this.TryValidateCompositeDataCooker(dataOutputPath/*, out var compositeCooker*/))
            {
                return GetCompositeDataRetrieval(dataOutputPath).TryQueryOutput<T>(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private object QuerySourceOutput(DataOutputPath dataOutputPath)
        {
            this.ValidateSourceDataCooker(dataOutputPath);
            return this.sourceCookerData.QueryOutput(dataOutputPath);
        }

        private object QueryCompositeOutput(DataOutputPath dataOutputPath)
        {
            this.ValidateCompositeDataCooker(dataOutputPath);
            return GetCompositeDataRetrieval(dataOutputPath).QueryOutput(dataOutputPath);
        }

        private bool TryQuerySourceOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (this.TryValidateSourceDataCooker(dataOutputPath))
            {
                return this.sourceCookerData.TryQueryOutput(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private bool TryQueryCompositeOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (this.TryValidateCompositeDataCooker(dataOutputPath))
            {
                return GetCompositeDataRetrieval(dataOutputPath).TryQueryOutput(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private ICookedDataRetrieval GetCompositeDataRetrieval(DataOutputPath outputPath)
        {
            return GetCompositeDataRetrieval(outputPath.CookerPath);
        }

        private ICookedDataRetrieval GetCompositeDataRetrieval(DataCookerPath cookerPath)
        {
            return this.compositeCookers.GetOrCreateCompositeCooker(
                cookerPath,
                this.createCompositeDataRetrieval);
        }
    }
}
