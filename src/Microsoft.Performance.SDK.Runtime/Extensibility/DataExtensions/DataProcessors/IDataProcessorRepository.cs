// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors
{
    /// <summary>
    ///     This interface provides access to a collection of data processors.
    /// </summary>
    public interface IDataProcessorRepository
    {
        /// <summary>
        ///     Gets the ids for all data processors in this repository.
        /// </summary>
        IEnumerable<DataProcessorId> DataProcessors { get; }

        /// <summary>
        ///     Returns a reference to a given data processor.
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Data processor Id.
        /// </param>
        /// <returns>
        ///     A data cooker reference.
        /// </returns>
        IDataProcessorReference GetDataProcessorReference(DataProcessorId dataProcessorId);
    }
}
