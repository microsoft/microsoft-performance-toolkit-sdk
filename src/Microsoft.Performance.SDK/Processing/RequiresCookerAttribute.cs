// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// This attribute is used on a class with static properties that return <see cref="TableDescriptor"/>
    /// or on individual <see cref="TableDescriptor"/> static properties to indicate that a 
    /// table described by the <see cref="TableDescriptor"/> requires a data cooker.
    /// </summary>
    public abstract class RequiresCookerAttribute
        : Attribute
    {
        // Placeholder so deriving classes can call a constructor. When this class is made abstract this can be removed.
        protected RequiresCookerAttribute()
        {
        }

        /// <summary>
        /// Path to a required data cooker for the given table.
        /// </summary>
        public DataCookerPath RequiredDataCookerPath { get; protected set; }
    }
}
