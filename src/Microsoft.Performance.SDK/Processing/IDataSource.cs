// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a source of data.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        ///     Gets the <see cref="Uri" /> of the data.
        /// </summary>
        /// <returns>
        ///     The URI of the data.
        /// </returns>
        Uri Uri { get; }
    }
}
