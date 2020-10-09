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
    ///     Creates data extensions.
    /// </summary>
    public interface IDataExtensionFactory
    {
        /// <summary>
        ///     Retrieves a singleton instance of an IDataExtensionRepositoryBuilder.
        /// </summary>
        IDataExtensionRepositoryBuilder SingletonDataExtensionRepository { get; }

        /// <summary>
        ///     Generate a source data cooker reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     data extension type.
        /// </param>
        /// <param name="reference">
        ///     data extension reference.
        /// </param>
        /// <returns>
        ///     true if succeeded.
        /// </returns>
        bool TryCreateSourceDataCookerReference(
            Type candidateType,
            out ISourceDataCookerReference reference);

        /// <summary>
        ///     Generate a composite data cooker reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        /// data extension type</param>
        /// <param name="reference">
        ///     Data extension reference.
        /// </param>
        /// <returns>
        ///     true if succeeded.
        /// </returns>
        bool TryCreateCompositeDataCookerReference(
            Type candidateType,
            out ICompositeDataCookerReference reference);

        /// <summary>
        ///     Generate a data processor reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     data extension type.
        /// </param>
        /// <param name="reference">
        ///     data extension reference.
        /// </param>
        /// <returns>
        ///     true if succeeded.
        /// </returns>
        bool TryCreateDataProcessorReference(
            Type candidateType,
            out IDataProcessorReference reference);

        /// <summary>
        ///     Generate a table reference from a given type.
        /// </summary>
        /// <param name="candidateType">
        ///     data extension type.
        /// </param>
        /// <param name="reference">
        ///     data extension reference.
        /// </param>
        /// <returns>
        ///     true if succeeded.
        /// </returns>
        bool TryCreateTableReference(
            Type candidateType,
            out ITableExtensionReference reference);

        /// <summary>
        ///     Creates a data extension repository.
        /// </summary>
        /// <returns>
        ///     Data extension repository.
        /// </returns>
        IDataExtensionRepositoryBuilder CreateDataExtensionRepository();

        /// <summary>
        ///     Creates a source session factory.
        /// </summary>
        /// <returns>
        ///     Source session factory.
        /// </returns>
        ISourceSessionFactory CreateSourceSessionFactory();

        /// <summary>
        ///     Creates an ICookedDataRetrieval object for retrieval across multiple processors.
        /// </summary>
        /// <param name="processors">
        ///     Custom data processors.
        /// </param>
        /// <returns>
        ///     Cooked data retrieval.
        /// </returns>
        ICookedDataRetrieval CreateCrossParserSourceDataCookerRetrieval(
            IEnumerable<ICustomDataProcessorWithSourceParser> processors);
    }
}
