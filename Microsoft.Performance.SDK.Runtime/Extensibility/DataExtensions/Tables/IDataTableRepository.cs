// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables
{
    /// <summary>
    ///     Provides access to a set of table data extensions.
    /// </summary>
    public interface IDataTableRepository
    {
        /// <summary>
        ///     Table extension references by table id.
        /// </summary>
        IReadOnlyDictionary<Guid, ITableExtensionReference> TablesById { get; }
    }
}
