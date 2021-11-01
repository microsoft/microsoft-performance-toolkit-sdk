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
        ///     Create an instance of the described data cooker.
        /// </summary>
        /// <param name="requiredData">
        ///     Data required by the data cooker.
        /// </param>
        /// <returns>
        ///     A composite data cooker.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="requiredData"/> is <c>null</c>.
        /// </exception>
        IDataCooker CreateInstance(IDataExtensionRetrieval requiredData);
    }
}
