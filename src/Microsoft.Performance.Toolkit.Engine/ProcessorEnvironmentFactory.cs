// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;


namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     This is used to generate processor environments.
    /// </summary>
    public abstract class ProcessorEnvironmentFactory
    {
        /// <summary>
        ///     Creates a <see cref="ProcessorEnvironment"/> for a processor.
        /// </summary>
        /// <param name="processingSourceIdentifier">
        ///     Identifies the source processor used to generate the data processor.
        /// </param>
        /// <param name="dataSourceGroup">
        ///     A collection of <see cref="IDataSource"/>s that a <see cref="ICustomDataProcessor"/> can process together
        ///     in a specified <see cref="IProcessingMode"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="ProcessorEnvironment"/> or <c>null</c>. When <c>null</c> is returned, a default processor 
        ///     environment will be used.
        /// </returns>
        public abstract ProcessorEnvironment CreateProcessorEnvironment(
            Guid processingSourceIdentifier,
            IDataSourceGroup dataSourceGroup);
    }
}
