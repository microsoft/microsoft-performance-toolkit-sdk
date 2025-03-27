// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Enumerates the different sources of serialized table configuration
    ///     data.
    /// </summary>
    public enum SerializationSource
    {
        /// <summary>
        ///     The tables configurations are pre-built and persisted.
        /// </summary>
        PrebuiltTableConfiguration,
    }

    /// <summary>
    ///     This interface is used to expose the tables associated
    ///     with processing a given data source.
    /// </summary>
    public interface IProcessingSource
    {
        /// <summary>
        ///     Gets the collection of tables exposed by this <see cref="IProcessingSource"/>
        ///     which are not marked as <see cref="TableDescriptor.IsMetadataTable"/>
        /// </summary>
        IEnumerable<TableDescriptor> DataTables { get; }

        /// <summary>
        ///     Gets the collection of tables exposed by this <see cref="IProcessingSource"/>
        ///     which are marked as <see cref="TableDescriptor.IsMetadataTable"/>
        /// </summary>
        IEnumerable<TableDescriptor> MetadataTables { get; }

        /// <summary>
        ///     Gets the options that are supported by this <see cref="IProcessingSource"/>.
        /// </summary>
        IEnumerable<Option> CommandLineOptions { get; }

        /// <summary>
        ///     Gets the <see cref="PluginOptionDefinition"/>s for plugin options that are supported by this <see cref="IProcessingSource"/>.
        /// </summary>
        /// <remarks>
        ///     Plugins can obtain a <see cref="PluginOptionValue"/> for each <see cref="PluginOptionDefinition"/> returned by
        ///     this property by querying <see cref="IApplicationEnvironmentV3.TryGetPluginOption{T}"/>.
        /// </remarks>
        IEnumerable<PluginOptionDefinition> PluginOptions { get; }

        /// <summary>
        ///     Gets information about this <see cref="IProcessingSource"/>.
        /// </summary>
        /// <returns>
        ///     The <see cref="ProcessingSourceInfo"/> for this <see cref="IProcessingSource"/>.
        /// </returns>
        ProcessingSourceInfo GetAboutInfo();

        /// <summary>
        ///     Sets the application environment for the <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="applicationEnvironment">
        ///     The application environment.
        /// </param>
        void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment);

        /// <summary>
        ///     Provides the <see cref="IProcessingSource"/> an application-appropriate logging mechanism.
        /// </summary>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        void SetLogger(ILogger logger);

        /// <summary>
        ///     Creates a new processor for processing the specified data source.
        /// </summary>
        /// <param name="dataSource">
        ///     The data source to process.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The command line options to pass to the processor.
        /// </param>
        /// <returns>
        ///     The created <see cref="ICustomDataProcessor"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/>, <paramref name="processorEnvironment"/>, or <paramref name="options"/> is <c>null</c>.
        /// </exception>
        ICustomDataProcessor CreateProcessor(
            IDataSource dataSource,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options);

        /// <summary>
        ///     Creates a new processor for processing the specified data sources.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to process.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The command line options to pass to the processor.
        /// </param>
        /// <returns>
        ///     The created <see cref="ICustomDataProcessor"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/>, <paramref name="processorEnvironment"/>, or <paramref name="options"/> is <c>null</c>.
        /// </exception>
        ICustomDataProcessor CreateProcessor(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options);
        
        /// <summary>
        ///     Creates a new processor for processing the specified data sources.
        /// </summary>
        /// <param name="dataSourceGroup">
        ///     The data source group to process.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The command line options to pass to the processor.
        /// </param>
        /// <returns>
        ///     The created <see cref="ICustomDataProcessor"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSourceGroup"/>, <paramref name="processorEnvironment"/>, or <paramref name="options"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This <see cref="IProcessingSource"/> implements <see cref="IDataSourceGrouper"/>, but does not provide a
        ///     valid a way to process a <see cref="IDataSourceGroup"/>.
        /// </exception>
        ICustomDataProcessor CreateProcessor(
            IDataSourceGroup dataSourceGroup,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options);

        /// <summary>
        ///     Retrieves a stream for serializing data. This method may return
        ///     <c>null</c>.
        ///     <para />
        ///     Source: PrebuiltTableConfiguration => TableConfigurations[]
        /// </summary>
        /// <param name="source">
        ///     Identifies the stream source.
        /// </param>
        /// <returns>
        ///     Serialization stream.
        /// </returns>
        Stream GetSerializationStream(SerializationSource source);

        /// <summary>
        ///     Returns a value indicating whether the given Data Source can
        ///     be opened by this instance.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source of interest.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance can actually process the given Data Source;
        ///     <c>false</c> otherwise.
        /// </returns>
        bool IsDataSourceSupported(IDataSource dataSource);

        /// <summary>
        ///     Provides a method for this <see cref="IProcessingSource"/> to do any
        ///     special cleanup operations for processors created by
        ///     this instance, if applicable.
        ///     <para />
        ///     It is guaranteed that the <paramref name="processor"/>
        ///     passed to this method was created by this instance.
        /// </summary>
        /// <param name="processor">
        ///     The processor to dispose.
        /// </param>
        void DisposeProcessor(ICustomDataProcessor processor);
    }
}
