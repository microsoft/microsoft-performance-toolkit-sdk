// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.Options;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Specifies a set of values that are used when
    ///     you create a <see cref="Engine"/>.
    /// </summary>
    public sealed class EngineCreateInfo
    {
        private static string DefaultRuntimeName;
        private IProcessingOptionsResolver optionsResolver;

        /// <summary>
        ///     Initializes the statc members of the <see cref="EngineCreateInfo"/>
        ///     class.
        /// </summary>
        static EngineCreateInfo()
        {
            EngineCreateInfo.DefaultRuntimeName = typeof(EngineCreateInfo).Assembly.GetName().Name;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <remarks>
        ///     Uses default ProcessorOptions. To modify this behavior <see cref="WithProcessingOptions(IProcessingOptionsResolver)"/>.
        /// </remarks>
        /// <param name="dataSources">
        ///     The data sources to be processed in the engine.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(ReadOnlyDataSourceSet dataSources)
        {
            Guard.NotNull(dataSources, nameof(dataSources));

            this.DataSources = dataSources;
            this.optionsResolver = GlobalProcessingOptionsResolver.Default;
        }

        /// <summary>
        ///     <see cref="ProcessorOptions"/> to use for all processing sources and data source groups during processing.
        /// </summary>
        /// <param name="globalProcessorOptions"> <see cref="ProcessorOptions"/> to use for all processing.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="globalProcessorOptions"/> is <c>null</c>.
        /// </exception>
        public void WithProcessorOptions(ProcessorOptions globalProcessorOptions)
        {
            Guard.NotNull(globalProcessorOptions, nameof(globalProcessorOptions));

            this.WithProcessingOptions(new GlobalProcessingOptionsResolver(globalProcessorOptions));
        }

        /// <summary>
        ///     Set <see cref="ProcessorOptions"/> per <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="processingSourceOptionsMap">A map of <see cref="IProcessingSource"/>s with <see cref="ProcessorOptions"/> to pass it.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="processingSourceOptionsMap"/> is <c>null</c>.
        /// </exception>
        public void WithProcessorOptions(IDictionary<IProcessingSource, ProcessorOptions> processingSourceOptionsMap)
        {
            Guard.NotNull(processingSourceOptionsMap, nameof(processingSourceOptionsMap));

            this.WithProcessingOptions(new ProcessingSourceOptionsResolver(processingSourceOptionsMap));
        }

        /// <summary>
        ///     Set a custom resolver for returning processor options. <see cref="IProcessingOptionsResolver"/> for more info. 
        /// </summary>
        /// <remarks>
        ///     To set global processor options, you can also use <see cref="WithProcessorOptions(ProcessorOptions)"/>.
        ///     To set processor options per processing source <seealso cref="WithProcessorOptions(IDictionary{IProcessingSource, ProcessorOptions})"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="processingOptionsResolver"/> is <c>null</c>.
        /// </exception>
        public void WithProcessingOptions(IProcessingOptionsResolver processingOptionsResolver)
        {
            this.optionsResolver = processingOptionsResolver;
        }

        /// <summary>
        ///     Gets or sets the name of the runtime on which the application is built.
        /// </summary>
        /// <remarks>
        ///     Defaults to the engine assembly name.
        /// </remarks>
        public string RuntimeName { get; set; } = EngineCreateInfo.DefaultRuntimeName;

        /// <summary>
        ///     Gets or sets the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets a logger factory.
        /// </summary>
        /// <remarks>
        ///     The logger factory should be able to provide each processing source a logger specific to its type.
        /// </remarks>
        public Func<Type, ILogger> LoggerFactory { get; set; }

        /// <summary>
        ///     Gets the data sources that are to be processed by an <see cref="Engine"/>
        ///     instance.
        /// </summary>
        public ReadOnlyDataSourceSet DataSources { get; }

        /// <summary>
        ///     
        /// </summary>
        public IProcessingOptionsResolver OptionsResolver => this.optionsResolver;

        /// <summary>
        ///     Gets or sets a value indicating whether the Engine instance will
        ///     allow for user interaction. If this property is set to <c>false</c>,
        ///     then any attempts made by plugins to get interaction from a user
        ///     will fail. By default, this property is <c>false</c>.
        /// </summary>
        public bool IsInteractive { get; set; }
    }
}
