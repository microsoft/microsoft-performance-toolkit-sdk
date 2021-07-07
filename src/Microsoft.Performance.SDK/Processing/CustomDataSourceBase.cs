// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        protected CustomDataSourceBase(
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider)
            : base(additionalTablesProvider)
        {
        }

        protected CustomDataSourceBase(
            Func<IDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>> additionalTablesProvider,
            Func<Assembly> tableAssemblyProvider)
            : base(additionalTablesProvider, tableAssemblyProvider)
        {
        }
    }
}
