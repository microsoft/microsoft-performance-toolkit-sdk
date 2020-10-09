// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    public interface IProvideColorTupleHash
    {
        /// <summary>
        /// Implemented on the type itself as a substitute for Object.GetHashCode() when 
        /// we need a color index that derives from the value of some data. (you should 
        /// not use pointer values to compute the color hash, for instance)
        /// This interface is analogous in pattern to IEquatable
        /// </summary>
        /// <returns>
        /// Returns color hash values (int, int?)
        /// Values Item1 and Item2 represent a color primary and secondary (optional) index value
        /// </returns>
        ValueTuple<int, int?> GetColorHash();
    }
}
