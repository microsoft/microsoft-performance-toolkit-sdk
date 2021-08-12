// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     For tables that are built through an <see cref="ICustomDataProcessor"/>, this is used to map a table to its
    ///     data processor.
    /// </summary>
    public interface IMapTablesToProcessors
    {
        /// <summary>
        ///     Maps a <see cref="TableDescriptor"/> to its <see cref="ICustomDataProcessor"/>.
        /// </summary>
        IReadOnlyDictionary<TableDescriptor, ICustomDataProcessor> TableToProcessor { get; }
    }
}
