// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     Used to retrieve a composite data cooker.
    /// </summary>
    public interface ICompositeDataCookerRetrieval
    {
        /// <summary>
        ///     Create an instance of the described data cooker, or use an
        ///     existing object if it has already been created.
        /// </summary>
        /// <param name="requiredData">
        ///     Data required by the data cooker.
        /// </param>
        /// <returns>
        ///     A data cooker.
        /// </returns>
        IDataCooker GetOrCreateInstance(IDataExtensionRetrieval requiredData);
    }
}
