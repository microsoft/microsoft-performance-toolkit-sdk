// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This interface is used to expose the tables associated
    ///     with processing a given data source.
    /// </summary>
    [Obsolete("ICustomDataSource will be renamed to IProcessingSource by v1.0.0 release candidate 1.")]
    public interface ICustomDataSource
        : IProcessingSource
    {
    }
}
