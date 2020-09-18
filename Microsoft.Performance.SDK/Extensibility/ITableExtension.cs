// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Basic information about a table extension.
    /// </summary>
    public interface ITableExtension
    {
        /// <summary>
        ///     Gets a table descriptor.
        /// </summary>
        TableDescriptor TableDescriptor { get; }

        /// <summary>
        ///     Gets a table build action.
        /// </summary>
        Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction { get; }

        /// <summary>
        ///     Gets a has data function for checking if a table has data. This property may be null.
        /// </summary>
        Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc { get; }
    }
}
