// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     An implementation of <see cref="IDataExtensionFactory"/>, used to create data extensions.
    /// </summary>
    public class DataExtensionFactory
        : IDataExtensionFactory
    {
        private static readonly DataExtensionRepository SingletonRepository 
            = new DataExtensionRepository();

        /// <summary>
        ///     Gets a singleton instance of this class.
        /// </summary>
        public IDataExtensionRepositoryBuilder SingletonDataExtensionRepository 
            => DataExtensionFactory.SingletonRepository;

        /// <summary>
        ///     Generate a source data cooker reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     Data extension type.
        /// </param>
        /// <param name="reference">
        ///     Data extension reference.
        /// </param>
        /// <returns>
        ///     <c>true</c> if succeeded, <c>false</c> otherwise.
        /// </returns>
        public bool TryCreateSourceDataCookerReference(
            Type candidateType,
            out ISourceDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            return SourceDataCookerReference.TryCreateReference(candidateType, out reference);
        }

        /// <summary>
        ///     Generate a composite data cooker reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     Data extension type.
        /// </param>
        /// <param name="reference">
        ///     Data extension reference.
        /// </param>
        /// <returns>
        ///     <c>true</c> if succeeded, <c>false</c> otherwise.
        /// </returns>
        public bool TryCreateCompositeDataCookerReference(
            Type candidateType,
            out ICompositeDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            return CompositeDataCookerReference.TryCreateReference(candidateType, out reference);
        }

        /// <summary>
        ///     Generate a data processor reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     Data extension type.
        /// </param>
        /// <param name="reference">
        ///     Data extension reference.
        /// </param>
        /// <returns>
        ///     <c>true</c> if succeeded, <c>false</c> otherwise.
        /// </returns>
        public bool TryCreateDataProcessorReference(
            Type candidateType,
            out IDataProcessorReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            return DataProcessorReference.TryCreateReference(candidateType, out reference);
        }

        /// <summary>
        ///     Generate a table reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     Data extension type.
        /// </param>
        /// <param name="reference">
        ///     Data extension reference.
        /// </param>
        /// <returns>
        ///     <c>true</c> if succeeded, <c>false</c> otherwise.
        /// </returns>
        public bool TryCreateTableReference(
            Type candidateType,
            out ITableExtensionReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            return TableExtensionReference.TryCreateReference(candidateType, out reference);
        }

        /// <summary>
        ///     Creates a data extension repository.
        /// </summary>
        /// <returns>
        ///     Data extension repository.
        /// </returns>
        public IDataExtensionRepositoryBuilder CreateDataExtensionRepository()
        {
            return new DataExtensionRepository();
        }

        /// <summary>
        ///     Creates a source session factory.
        /// </summary>
        /// <returns>
        ///     Source session factory.
        /// </returns>
        public ISourceSessionFactory CreateSourceSessionFactory()
        {
            return new SourceProcessingSessionFactory();
        }

        /// <summary>
        ///     Creates an ICookedDataRetrieval object for retrieval across multiple processors.
        /// </summary>
        /// <param name="processors">
        ///     Custom data processors.
        /// </param>
        /// <returns>
        ///     Cooked data retrieval.
        /// </returns>
        public ICookedDataRetrieval CreateCrossParserSourceDataCookerRetrieval(
            IEnumerable<ICustomDataProcessorWithSourceParser> processors)
        {
            Guard.NotNull(processors, nameof(processors));

            return new CrossParserSourceDataCookerRetrieval(processors);
        }
    }
}
