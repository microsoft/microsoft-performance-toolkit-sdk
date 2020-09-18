// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a mechanism for transforming elements of a collection into a different type. This is used
    ///     transform rows from a hierarchical column into sub-elements.
    /// </summary>
    /// <typeparam name="TCollection">Input type to be transformed into <c>T</c></typeparam>
    /// <typeparam name="T">Output type</typeparam>
    public interface ICollectionAccessProvider<TCollection, T> 
        : ICollectionInfoProvider<TCollection>,
          IEqualityComparer<TCollection>
    {
        /// <summary>
        /// A constant that is not a member of any collection.
        ///
        /// When 0 &gt;= index &lt; GetCount(),
        /// GetValue(collection, index).CompareTo(PastEndValue) must not be 0.
        /// 
        /// If a collection may include the default value, use a non-default
        /// value for PastEndValue.
        /// 
        /// x.CompareTo() must properly test for equality of x with
        /// PastEndValue. x.CompareTo() does not need to test whether x is
        /// less than or greater than PastEndValue.
        /// 
        /// Column cells show PastEndValue.ToString() when data is 
        /// unavailable. Consider meaningful output such as "n/a".
        /// </summary>
        T PastEndValue
        {
            get;
        }

        /// <summary>
        ///     Gets the parent of the given collection.
        /// </summary>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        /// <returns>
        ///     The parent of the collection.
        /// </returns>
        TCollection GetParent(TCollection collection);

        /// <summary>
        /// The data engine calls this function for all collections that return
        /// !IsNull(collection), regardless of the result of
        /// GetCount(collection). The purpose of this quirk is to allow access
        /// providers to assign special values to special collections, Idle
        /// stacks for example.
        /// 
        /// Special value collections work correctly if HasUniqueStart()
        /// returns false!
        /// 
        /// The dataengine calls this function once for IsNull(collection) to
        /// get the value of a null collection.
        /// 
        /// Returns the value of element in the collection if index is
        /// within bounds; otherwise if index is out-of-bounds, returns
        /// PastEndValue.
        /// </summary>
        T GetValue(TCollection collection, int index);
    }
}
