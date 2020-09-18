// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors
{
    /// <summary>
    ///     A wrapper around a type that is a data processor.
    /// </summary>
    public interface IDataProcessorReference
        : IDataProcessorCreator,
          IDataProcessorDescriptor,
          IDataExtensionReference
    {
    }
}
