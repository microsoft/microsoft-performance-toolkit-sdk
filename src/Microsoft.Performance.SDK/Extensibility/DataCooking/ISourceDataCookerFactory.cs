// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     An interface to describe and instantiate a given data cooker.
    /// </summary>
    public interface ISourceDataCookerFactory
    {
        /// <summary>
        ///     Create a new instance of the described data cooker.
        /// </summary>
        /// <returns>
        ///     A data cooker.
        /// </returns>
        IDataCookerDescriptor CreateInstance();
    }
}
