// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// When implemented by a custom data processor, this enables a form of dynamically generated table. These tables
    /// are created while processing the data source.
    /// </summary>
    /// <remarks>
    /// These tables will not show up in some table lists, as these tables won't always be available from the given
    /// custom data source.
    /// </remarks>
    public interface IDataDerivedTables
    {
        /// <summary>
        /// These tables are generated while processing the data source.
        /// </summary>
        IReadOnlyCollection<TableDescriptor> DataDerivedTables
        {
            get;
        }
    }
}
