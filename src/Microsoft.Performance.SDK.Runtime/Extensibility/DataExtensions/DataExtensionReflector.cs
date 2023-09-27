// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This class iterates over types, loading them into an IDataExtensionRepositoryBuilder.
    /// </summary>
    public class DataExtensionReflector
        : IExtensionTypeObserver
    {
        private readonly IDataExtensionRepositoryBuilder repoBuilder;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the DataExtensionReflector class.
        /// </summary>
        /// <param name="extensionProvider">
        ///     A type provider that this observer will register with.
        /// </param>
        /// <param name="repoBuilder">
        ///     Provides an output to store discovered extensions.
        /// </param>
        /// <param name="logger">
        ///     Logs messages during reference creation.
        /// </param>
        public DataExtensionReflector(
            IExtensionTypeProvider extensionProvider,
            IDataExtensionRepositoryBuilder repoBuilder,
            ILogger logger)
        {
            if (logger == null)
            {
                logger = Logger.Create<DataExtensionReflector>();
            }

            this.repoBuilder = repoBuilder;
            this.logger = logger;

            extensionProvider.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public void ProcessType(Type type, string sourceName)
        {
            // Find source data cookers
            if (SourceDataCookerReference.TryCreateReference(type, this.logger, out var sourceDataCookerReference))
            {
                Debug.Assert(sourceDataCookerReference != null);
                this.repoBuilder.AddSourceDataCookerReference(sourceDataCookerReference);
            }

            // Find composite data cookers
            if (CompositeDataCookerReference.TryCreateReference(type, this.logger, out var compositeDataCookerReference))
            {
                Debug.Assert(compositeDataCookerReference != null);
                this.repoBuilder.AddCompositeDataCookerReference(compositeDataCookerReference);
            }

            // Find tables
            if (TableExtensionReference.TryCreateReference(type, this.logger, out var dataExtensionReference))
            {
                Debug.Assert(dataExtensionReference != null);
                this.repoBuilder.AddTableExtensionReference(dataExtensionReference);
            }

            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////// Find data processors
            ////if (DataProcessorReference.TryCreateReference(type, out var dataProcessorReference))
            ////{
            ////    Debug.Assert(dataProcessorReference != null);
            ////    this.repoBuilder.AddDataProcessorReference(dataProcessorReference);
            ////}
        }

        /// <inheritdoc />
        public void DiscoveryStarted()
        {
            // nothing needed here
        }

        /// <inheritdoc />
        public void DiscoveryComplete()
        {
            // nothing needed here
        }
    }
}
