// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     This class is used to retrieve data from the subset of all available data extension objects. It is used to
    ///     limit the data available to a given data extension, based on the requirements declared by that data extension.
    /// </summary>
    internal class FilteredDataRetrieval
        : IDataExtensionRetrieval
    {
        private readonly DataExtensionRetrievalFactory dataRetrievalFactory;
        private readonly IDataExtensionDependencies extensionDependencies;

        internal FilteredDataRetrieval(
            DataExtensionRetrievalFactory dataRetrievalFactory,
            IDataExtensionDependencies extensionDependencies)
        {
            Debug.Assert(dataRetrievalFactory != null, nameof(dataRetrievalFactory));
            Debug.Assert(extensionDependencies != null, nameof(extensionDependencies));

            this.dataRetrievalFactory = dataRetrievalFactory;
            this.extensionDependencies = extensionDependencies;
        }

        /// <inheritdoc/>
        public T QueryOutput<T>(DataOutputPath dataOutputPath)
        {
            if (!string.IsNullOrWhiteSpace(dataOutputPath.SourceParserId))
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
            if (!string.IsNullOrWhiteSpace(dataOutputPath.SourceParserId))
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
            if (!string.IsNullOrWhiteSpace(dataOutputPath.SourceParserId))
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
            if (!string.IsNullOrWhiteSpace(dataOutputPath.SourceParserId))
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

            var processorReference = this.dataRetrievalFactory.DataExtensionRepository.GetDataProcessorReference(dataProcessorId);
            if (processorReference == null)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve a reference to data processor: {dataProcessorId}.");
            }

            var processorDataRetrieval =
                this.dataRetrievalFactory.CreateDataRetrievalForDataProcessor(dataProcessorId);

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

        private IDataCooker ValidateCompositeDataCooker(DataOutputPath dataOutputPath)
        {
            var cookerPath = dataOutputPath.CookerPath;

            if (!this.extensionDependencies.RequiredCompositeDataCookerPaths.Contains(cookerPath))
            {
                throw new ArgumentException(
                    $"The requested data cooker is not available: {dataOutputPath}. " +
                    "Consider adding it to the requirements for this data extension.",
                    nameof(dataOutputPath));
            }

            var compositeCookerReference =
                this.dataRetrievalFactory.DataExtensionRepository.GetCompositeDataCookerReference(cookerPath);
            if (compositeCookerReference == null)
            {
                throw new InvalidOperationException(
                    $"The data extension repository is missing expected composite data cooker: {cookerPath}");
            }

            var compositeCooker = compositeCookerReference.GetOrCreateInstance(
                this.dataRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath));
            if (compositeCooker == null)
            {
                throw new InvalidOperationException(
                    $"The composite cooker reference returned null cooker: {cookerPath}. Was it properly initialized?");
            }

            return compositeCooker;
        }

        private bool TryValidateCompositeDataCooker(DataOutputPath dataOutputPath, out IDataCooker compositeCooker)
        {
            try
            {
                var cookerPath = dataOutputPath.CookerPath;

                if (!this.extensionDependencies.RequiredCompositeDataCookerPaths.Contains(cookerPath))
                {
                    compositeCooker = default;
                    return false;
                }

                var compositeCookerReference =
                    this.dataRetrievalFactory.DataExtensionRepository.GetCompositeDataCookerReference(cookerPath);
                if (compositeCookerReference == null)
                {
                    compositeCooker = default;
                    return false;
                }

                compositeCooker = compositeCookerReference.GetOrCreateInstance(
                    this.dataRetrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath));

                return compositeCooker != null;
            }
            catch (Exception)
            {
            }

            compositeCooker = default;
            return false;
        }

        private T QuerySourceOutput<T>(DataOutputPath dataOutputPath)
        {
            this.ValidateSourceDataCooker(dataOutputPath);
            return this.dataRetrievalFactory.CookedSourceData.QueryOutput<T>(dataOutputPath);
        }

        private bool TryQuerySourceOutput<T>(DataOutputPath dataOutputPath, out T result)
        {
            if (this.TryValidateSourceDataCooker(dataOutputPath))
            {
                return this.dataRetrievalFactory.CookedSourceData.TryQueryOutput<T>(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private T QueryCompositeOutput<T>(DataOutputPath dataOutputPath)
        {
            var compositeCooker = this.ValidateCompositeDataCooker(dataOutputPath);
            return compositeCooker.QueryOutput<T>(dataOutputPath);
        }

        private bool TryQueryCompositeOutput<T>(DataOutputPath dataOutputPath, out T result)
        {
            if (this.TryValidateCompositeDataCooker(dataOutputPath, out var compositeCooker))
            {
                return compositeCooker.TryQueryOutput<T>(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private object QuerySourceOutput(DataOutputPath dataOutputPath)
        {
            this.ValidateSourceDataCooker(dataOutputPath);
            return this.dataRetrievalFactory.CookedSourceData.QueryOutput(dataOutputPath);
        }

        private object QueryCompositeOutput(DataOutputPath dataOutputPath)
        {
            var compositeCooker = this.ValidateCompositeDataCooker(dataOutputPath);
            return compositeCooker.QueryOutput(dataOutputPath);
        }

        private bool TryQuerySourceOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (this.TryValidateSourceDataCooker(dataOutputPath))
            {
                return this.dataRetrievalFactory.CookedSourceData.TryQueryOutput(dataOutputPath, out result);
            }

            result = default;
            return false;
        }

        private bool TryQueryCompositeOutput(DataOutputPath dataOutputPath, out object result)
        {
            if (this.TryValidateCompositeDataCooker(dataOutputPath, out var compositeCooker))
            {
                return compositeCooker.TryQueryOutput(dataOutputPath, out result);
            }

            result = default;
            return false;
        }
    }
}
