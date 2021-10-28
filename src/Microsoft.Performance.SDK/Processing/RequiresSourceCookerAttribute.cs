// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This attribute is used on a class with static properties that return <see cref="TableDescriptor"/>
    ///     or on individual <see cref="TableDescriptor"/> static properties to indicate that a 
    ///     table described by the <see cref="TableDescriptor"/> requires the identified source data cooker.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RequiresSourceCookerAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiresSourceCookerAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="sourceParserId">
        ///     The ID of the source parser.
        /// </param>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     One or more of the parameters is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        public RequiresSourceCookerAttribute(string sourceParserId, string dataCookerId)
        {
            this.RequiredDataCookerPath = DataCookerPath.ForSource(sourceParserId, dataCookerId);
        }

        /// <summary>
        ///     Gets the path to a required data cooker for the given table.
        /// </summary>
        public DataCookerPath RequiredDataCookerPath { get;  }
    }
}
