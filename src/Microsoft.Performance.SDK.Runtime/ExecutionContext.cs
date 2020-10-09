// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents the context of execution for
    ///     a custom data source.
    /// </summary>
    public sealed class ExecutionContext
        : IFormattable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExecutionContext"/>
        ///     class.
        /// </summary>
        public ExecutionContext(
            IProgress<int> progress,
            Func<ICustomDataProcessor, ILogger> loggerFactory,
            ICustomDataSource customDataSource,
            IEnumerable<IDataSource> dataSources,
            IEnumerable<TableDescriptor> tablesToEnable,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions commandLineOptions)
        {
            Guard.NotNull(progress, nameof(progress));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(customDataSource, nameof(customDataSource));
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.All(dataSources, x => x != null, nameof(dataSources));
            Guard.NotNull(tablesToEnable, nameof(tablesToEnable));
            Guard.All(tablesToEnable, x => x != null, nameof(tablesToEnable));
            Guard.NotNull(processorEnvironment, nameof(processorEnvironment));
            Guard.NotNull(commandLineOptions, nameof(commandLineOptions));

            this.ProgressReporter = progress;
            this.LoggerFactory = loggerFactory;
            this.CustomDataSource = customDataSource;
            this.DataSources = dataSources.ToList().AsReadOnly();
            this.TablesToEnable = tablesToEnable.ToList().AsReadOnly();
            this.ProcessorEnvironment = processorEnvironment;
            this.CommandLineOptions = commandLineOptions;
        }

        /// <summary>
        ///     Gets the instance to use to report
        ///     processing progress.
        /// </summary>
        public IProgress<int> ProgressReporter { get; }

        /// <summary>
        ///     Gets the logger factory for the processor.
        /// </summary>
        public Func<ICustomDataProcessor, ILogger> LoggerFactory { get; }

        /// <summary>
        ///     Gets the <see cref="ICustomDataSource"/> associated
        ///     with the given data items.
        /// </summary>
        public ICustomDataSource CustomDataSource { get; }

        /// <summary>
        ///     Gets the <see cref="IDataSource"/>s that can be
        ///     processed by the <see cref="CustomDataSource"/>.
        /// </summary>
        public IEnumerable<IDataSource> DataSources { get; }

        /// <summary>
        ///     Gets the collection of tables that are to be
        ///     enabled for the processing session.
        /// </summary>
        public IEnumerable<TableDescriptor> TablesToEnable { get; }

        /// <summary>
        ///     The environment for the processor.
        /// </summary>
        public IProcessorEnvironment ProcessorEnvironment { get; }

        /// <summary>
        ///     Gets the collection of command line parameters to pass
        ///     to the processor.
        /// </summary>
        public ProcessorOptions CommandLineOptions { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToString("G");
        }

        /// <inheritdoc/>
        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Gets the string representation of this instance.
        /// </summary>
        /// <param name="format">
        ///     The format specifier describing how to format the instance.
        ///     <para/>
        ///     F - format as a comma separated list of file paths.
        ///     <para/>
        ///     G - The default format. Returns the name of the custom data source.
        /// </param>
        /// <param name="formatProvider">
        ///     An object to provide formatting information. This parameter may be <c>null</c>.
        /// </param>
        /// <returns>
        ///     The string representation of this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case "F":
                    return string.Join(",", this.DataSources.Select(x => x.GetUri()));

                case "G":
                case "g":
                case null:
                    return this.CustomDataSource.TryGetName();

                default:
                    throw new FormatException($"Unsupported format specified: '{format}'");
            }
        }
    }
}
