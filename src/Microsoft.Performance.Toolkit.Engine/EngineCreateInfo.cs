// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

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
        ///     Thrown is <paramref name="dataSources"/> is <c>null</c>.
        /// </exception>
        public EngineCreateInfo(DataSourceSet dataSources)
        {
            Guard.NotNull(dataSources, nameof(dataSources));

            this.DataSources = dataSources;
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
        public DataSourceSet DataSources { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the created <see cref="Engine"/>
        ///     instance owns the data sources passed in <see cref="DataSources"/>.
        ///     If this parameter is set to <c>true</c>, then when the <see cref="Engine"/>
        ///     instance is disposed, the <see cref="DataSources"/> instance will be disposed.
        ///     Set this property to <c>false</c> if you wish to use the same <see cref="DataSourceSet" />
        ///     across multiple <see cref="Engine"/> instances.
        ///     <para />
        ///     The default value is <c>true</c>.
        /// </summary>
        public bool OwnsDataSources { get; set; } = true;
    }
}
