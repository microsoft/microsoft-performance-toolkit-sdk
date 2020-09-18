// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataProcessing
{
    /// <summary>
    ///     Defines properties to describe a data processor.
    /// </summary>
    public interface IDataProcessorDescriptor
    {
        /// <summary>
        ///     Gets a unique identifier for a data processor.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a description of the data processor.
        /// </summary>
        string Description { get; }
    }
}
