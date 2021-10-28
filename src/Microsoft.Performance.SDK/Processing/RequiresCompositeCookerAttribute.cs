// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This attribute is used on a class with static properties that return <see cref="TableDescriptor"/>
    ///     or on individual <see cref="TableDescriptor"/> static properties to indicate that a 
    ///     table described by the <see cref="TableDescriptor"/> requires the identified composite data cooker.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequiresCompositeCookerAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiresCompositeCookerAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="dataCookerId">
        ///     The ID of the composite data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataCookerId"/> is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataCookerId"/> is null.
        /// </exception>
        public RequiresCompositeCookerAttribute(string dataCookerId)
        {
            this.RequiredDataCookerPath = DataCookerPath.ForComposite(dataCookerId);
        }

        /// <summary>
        ///     Gets the path to a required data cooker for the given table.
        /// </summary>
        public DataCookerPath RequiredDataCookerPath { get; }
    }
}
