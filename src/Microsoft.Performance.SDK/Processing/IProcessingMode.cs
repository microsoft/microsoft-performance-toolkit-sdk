// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     A mode of processing 1 or more <see cref="IDataSource" />s in a single call to
    ///     <see cref="ICustomDataProcessor.ProcessAsync" />.
    /// </summary>
    public interface IProcessingMode
    {
        /// <summary>
        ///     Gets a globally-unique identifier for this processing mode.
        /// </summary>
        Guid Guid { get; }
        
        /// <summary>
        ///     Gets the human-readable name of this processing mode.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets a human-readable description of this processing mode.
        /// </summary>
        string Description { get; }
    }
}