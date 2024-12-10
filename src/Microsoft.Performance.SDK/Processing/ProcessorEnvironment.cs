// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <inheritdoc cref="IProcessorEnvironment"/>
    public abstract class ProcessorEnvironment
        : IProcessorEnvironmentV2
    {
        /// <inheritdoc/>
        public abstract ILogger CreateLogger(Type processorType);

        /// <inheritdoc />
        /// <remarks>
        ///     This implementation does not support the concept of dynamic table builder and always returns 
        ///     <c>null</c>.
        /// </remarks>
        public virtual IDynamicTableBuilder RequestDynamicTableBuilder(TableDescriptor descriptor)
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual IProcessorTableDataSynchronizationFactory TableDataSynchronizerFactory
        {
            get
            {
                return null;
            }

            protected set
            {
            }
        }
    }
}
