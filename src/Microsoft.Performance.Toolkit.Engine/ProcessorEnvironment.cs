// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine
{
    public abstract class ProcessorEnvironment
        : IProcessorEnvironment
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
    }
}
