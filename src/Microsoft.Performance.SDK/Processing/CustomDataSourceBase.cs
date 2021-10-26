// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <remarks>
    ///     This class will be deleted prior to SDK v1.0.0 release candidate 1. It is
    ///     currently not sealed to maintain backwards compatability with existing plugins.
    /// </remarks>
    [Obsolete("CustomDataSourceBase will be renamed to ProcessingSource by v1.0.0 release candidate 1.")]
    public abstract class CustomDataSourceBase
        : ProcessingSource
    {
        protected CustomDataSourceBase()
            : base()
        {
        }
    }
}
