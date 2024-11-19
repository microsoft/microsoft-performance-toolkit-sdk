// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.Toolkit.Engine
{
    internal sealed class RuntimeProcessorEnvironmentFactory
        : ProcessorEnvironmentFactory
    {
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly ProcessorEnvironmentFactory wrappedFactory;

        public RuntimeProcessorEnvironmentFactory(Func<Type, ILogger> loggerFactory, ProcessorEnvironmentFactory wrappedFactory)
        {
            Debug.Assert(loggerFactory != null);
            this.loggerFactory = loggerFactory;
            this.wrappedFactory = wrappedFactory;
        }

        public override ProcessorEnvironment CreateProcessorEnvironment(
            Guid processingSourceIdentifier,
            IDataSourceGroup dataSourceGroup)
        {
            return this.wrappedFactory?.CreateProcessorEnvironment(processingSourceIdentifier, dataSourceGroup)
                ?? new RuntimeProcessorEnvironment(this.loggerFactory);
        }

        private sealed class RuntimeProcessorEnvironment
            : ProcessorEnvironment
        {
            private readonly Func<Type, ILogger> loggerFactory;
            private readonly object loggerLock = new object();

            private ILogger logger;
            private Type processorType;

            public RuntimeProcessorEnvironment(
                Func<Type, ILogger> loggerFactory)
            {
                Debug.Assert(loggerFactory != null);

                this.loggerFactory = loggerFactory;
            }

            public override ILogger CreateLogger(Type processorType)
            {
                Guard.NotNull(processorType, nameof(processorType));

                lock (this.loggerLock)
                {
                    if (logger != null)
                    {
                        if (this.processorType != processorType)
                        {
                            throw new ArgumentException(
                                $"{nameof(CreateLogger)} cannot be called with multiple types in a single instance.",
                                nameof(processorType));
                        }

                        return this.logger;
                    }

                    this.processorType = processorType;
                    this.logger = this.loggerFactory(processorType);
                    return this.logger;
                }
            }
        }
    }
}
