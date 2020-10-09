// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     Provides information about a data cooker.
    /// </summary>
    public interface IDataCookerDescriptor
    {
        /// <summary>
        ///     Gets a description of the data extension.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets the path to this data cooker.
        /// </summary>
        DataCookerPath Path { get; }
    }
}
