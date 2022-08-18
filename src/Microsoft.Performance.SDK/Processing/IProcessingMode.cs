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

        /// <summary>
        ///     Gets a value indicating if any <see cref="IDataSourceGroup" /> processed with this processing mode
        ///     can be combined with other processed <see cref="IDataSourceGroup" />s after processing is complete. Here,
        ///     "combined" refers to correlating <see cref="IDataSource" /> data through the <see cref="DataSourceInfo" />s
        ///     returned by separate groups' <see cref="ICustomDataProcessor.ProcessAsync" /> calls.
        /// </summary>
        /// <example>
        ///     If this processing mode corrupts time information, this value should be <c>false</c> since combining
        ///     its output with another <see cref="IDataSourceGroup" />'s output would result in mis-correlated
        ///     time information on the combined <see cref="IDataSourceGroup" />.
        /// </example>
        bool SupportsGroupCombining { get; }
    }
}