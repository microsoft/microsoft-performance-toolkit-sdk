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
        ///     Gets the <see cref="Uri"/> that contains the location
        ///     of the data.
        /// </summary>
        /// <returns>
        ///     The <see cref="Uri"/> of the data.
        /// </returns>
        Uri GetUri();
    }
}
