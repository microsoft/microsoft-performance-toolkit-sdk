// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Encapsulates information about an unexpected error in a <see cref="CustomDataProcessor"/>
    ///     or extension that occurred while the <see cref="Engine"/> was processing data.
    /// </summary>
    public sealed class ProcessingError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingError"/>
        ///     class.
        /// </summary>
        /// <param name="processingSourceGuid">
        ///     The GUID identifying the <see cref="IProcessingSource"/> that is
        ///     the source of the given <paramref name="processor"/>.
        /// </param>
        /// <param name="processor">
        ///     The <see cref="ICustomDataProcessor"/> that threw the error.
        /// </param>
        /// <param name="dataSources">
        ///     The <see cref="IDataSource"/>s that were being processed by
        ///     <paramref name="processor"/> when the error occurred.
        /// </param>
        /// <param name="processFault">
        ///     The error that was thrown.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="processor"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processFault"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="processingSourceGuid"/> is equivalent to the default Guid
        ///     (<see cref="Guid.Empty"/>).
        /// </exception>
        public ProcessingError(
            Guid processingSourceGuid,
            ICustomDataProcessor processor,
            IEnumerable<IDataSource> dataSources,
            Exception processFault)
        {
            Guard.NotNull(processor, nameof(processor));
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processFault, nameof(processFault));

            if (processingSourceGuid == default(Guid))
            {
                throw new ArgumentException($"The default GUID `{default(Guid)}` is not allowed.", nameof(processingSourceGuid));
            }

            this.Processor = processor;
            this.DataSources = dataSources.OfType<IDataSource>().ToList().AsReadOnly();
            this.ProcessFault = processFault;

            this.ProcessingSourceGuid = processingSourceGuid;
            this.DataSourceInfoFault = null;
            this.EnableTableFaults = new Dictionary<TableDescriptor, Exception>();
            this.ProcessFault = null;
        }

        internal ProcessingError(ExecutionResult executionResult)
        {
            Debug.Assert(executionResult != null);

            this.Processor = executionResult.Processor;
            this.DataSources = executionResult.Context.DataSources.ToList().AsReadOnly();

            this.ProcessingSourceGuid = executionResult.Context.ProcessingSource.Guid;
            this.DataSourceInfoFault = executionResult.DataSourceInfoFailure;
            this.EnableTableFaults = executionResult.EnableFailures;
            this.ProcessFault = executionResult.ProcessorFault;
        }

        /// <summary>
        ///     Gets the GUID that identifies the <see cref="IProcessingSource"/>
        ///     that is the source of the referenced <see cref="Processor"/>.
        /// </summary>
        public Guid ProcessingSourceGuid { get; }

        /// <summary>
        ///     Gets the <see cref="ICustomDataProcessor"/> that threw the error.
        /// </summary>
        public ICustomDataProcessor Processor { get; }

        /// <summary>
        ///     Gets the collection of <see cref="IDataSource"/>s that were being processed
        ///     by <see cref="Processor"/> when the error occurred.
        /// </summary>
        public IReadOnlyCollection<IDataSource> DataSources { get; }

        /// <summary>
        ///     Gets the error that occurred while retreiving data source information, 
        ///     if any occurred. This property may be <c>null</c>.
        /// </summary>
        public Exception DataSourceInfoFault { get; }

        /// <summary>
        ///     Gets the error(s) that occurred while enabling tables, if any occurred.
        ///     This property may be empty.
        /// </summary>
        public IReadOnlyDictionary<TableDescriptor, Exception> EnableTableFaults { get; }

        /// <summary>
        ///     Gets the error that was thrown by <see cref="Processor"/> while processing.
        /// </summary>
        public Exception ProcessFault { get; }
    }
}
