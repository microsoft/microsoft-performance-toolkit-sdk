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
        /// <param name="dataSources">
        ///     The data sources to be processed in the engine.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(ReadOnlyDataSourceSet dataSources) 
            : this(dataSources, GlobalProcessingOptionsResolver.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to be processed in the engine.
        /// </param>
        /// <param name="globalOptions">
        ///     Options to be passed to the <see cref="IProcessingOptionsResolver"/> for all Plugins and DataSources.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     <paramref name="globalOptions"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(ReadOnlyDataSourceSet dataSources, ProcessorOptions globalOptions) 
            : this(dataSources, new GlobalProcessingOptionsResolver(globalOptions))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to be processed in the engine.
        /// </param>
        /// <param name="processingSourceOptions">
        ///     A map to specify <see cref="ProcessorOptions"/> for each <see cref="IProcessingSource"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     <paramref name="processingSourceOptions"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(ReadOnlyDataSourceSet dataSources, IDictionary<IProcessingSource, ProcessorOptions> processingSourceOptions) 
            : this(dataSources, new ProcessingSourceOptionsResolver(processingSourceOptions))
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to be processed in the engine.
        /// </param>
        /// <param name="resolver">
        ///     Options Resolver <see cref="IProcessingOptionsResolver"/> which specifies <see cref="ProcessorOptions"/> for a DataSourceGroup and ProcessingSource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     <paramref name="optionsResolver"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(ReadOnlyDataSourceSet dataSources, IProcessingOptionsResolver optionsResolver)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(optionsResolver, nameof(optionsResolver));

            this.DataSources = dataSources;
            this.OptionsResolver = optionsResolver;
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
        public IProcessingOptionsResolver OptionsResolver { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Engine instance will
        ///     allow for user interaction. If this property is set to <c>false</c>,
        ///     then any attempts made by plugins to get interaction from a user
        ///     will fail. By default, this property is <c>false</c>.
        /// </summary>
        public bool IsInteractive { get; set; }
    }
}
