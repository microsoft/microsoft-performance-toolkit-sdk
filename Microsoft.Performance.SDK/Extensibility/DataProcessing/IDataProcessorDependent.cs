// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Extensibility.DataProcessing
{
    /// <summary>
    ///     Should be applied to any data extension type that may require data processors.
    /// </summary>
    public interface IDataProcessorDependent
    {
        /// <summary>
        ///     Gets the required data cooker processors.
        /// </summary>
        IReadOnlyCollection<DataProcessorId> RequiredDataProcessors { get; }
    }
}
