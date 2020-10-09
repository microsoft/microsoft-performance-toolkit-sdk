// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;
using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This class iterates over types, loading them into an IDataExtensionRepositoryBuilder.
    /// </summary>
    public class DataExtensionReflector
        : IExtensionTypeObserver
    {
        private readonly IDataExtensionRepositoryBuilder repoBuilder;

        /// <summary>
        ///     Initializes a new instance of the DataExtensionReflector class.
        /// </summary>
        /// <param name="extensionProvider">
        ///     A type provider that this observer will register with.
        /// </param>
        /// <param name="repoBuilder">
        ///     Provides an output to store discovered extensions.
        /// </param>
        public DataExtensionReflector(
            IExtensionTypeProvider extensionProvider,
            IDataExtensionRepositoryBuilder repoBuilder)
        {
            this.repoBuilder = repoBuilder;
            extensionProvider.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public void ProcessType(Type type, string sourceName)
        {
            // Find source data cookers
            if (SourceDataCookerReference.TryCreateReference(type, out var sourceDataCookerReference))
            {
                Debug.Assert(sourceDataCookerReference != null);
                this.repoBuilder.AddSourceDataCookerReference(sourceDataCookerReference);
            }

            // Find composite data cookers
            if (CompositeDataCookerReference.TryCreateReference(type, out var compositeDataCookerReference))
            {
                Debug.Assert(compositeDataCookerReference != null);
                this.repoBuilder.AddCompositeDataCookerReference(compositeDataCookerReference);
            }

            // Find tables
            if (TableExtensionReference.TryCreateReference(type, out var dataExtensionReference))
            {
                Debug.Assert(dataExtensionReference != null);
                this.repoBuilder.AddTableExtensionReference(dataExtensionReference);
            }

            // Find data processors
            if (DataProcessorReference.TryCreateReference(type, out var dataProcessorReference))
            {
                Debug.Assert(dataProcessorReference != null);
                this.repoBuilder.AddDataProcessorReference(dataProcessorReference);
            }
        }

        /// <inheritdoc />
        public void DiscoveryComplete()
        {
            // nothing needed here
        }
    }
}
