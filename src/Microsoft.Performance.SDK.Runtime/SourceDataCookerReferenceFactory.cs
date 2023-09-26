// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Provides a way to manually create a source cooker reference.
    /// </summary>
    public static class SourceDataCookerReferenceFactory
    {
        /// <summary>
        ///     Creates an <see cref="ISourceDataCookerReference"/> for a given type.
        /// </summary>
        /// <remarks>
        ///     This will not add the generated reference to a cooker repository, and the cooker's data will not be
        ///     available through the standard cooker framework.
        /// </remarks>
        /// <param name="candidateType">
        ///     Type that represents a source data cooker.
        /// </param>
        /// <param name="reference">
        ///     A reference to a source data cooker for the given type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a reference was generated; otherwise <c>false</c>
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out ISourceDataCookerReference reference)
        {
            return TryCreateReference(candidateType, Logger.Null, out reference);
        }

        /// <summary>
        ///     Creates an <see cref="ISourceDataCookerReference"/> for a given type.
        /// </summary>
        /// <remarks>
        ///     This will not add the generated reference to a cooker repository, and the cooker's data will not be
        ///     available through the standard cooker framework.
        /// </remarks>
        /// <param name="candidateType">
        ///     Type that represents a source data cooker.
        /// </param>
        /// <param name="logger">
        ///     Logs messages during reference creation.
        /// </param>
        /// <param name="reference">
        ///     A reference to a source data cooker for the given type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a reference was generated; otherwise <c>false</c>
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            ILogger logger,
            out ISourceDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));
            Guard.NotNull(logger, nameof(logger));

            return SourceDataCookerReference.TryCreateReference(candidateType, logger, out reference);
        }
    }
}
