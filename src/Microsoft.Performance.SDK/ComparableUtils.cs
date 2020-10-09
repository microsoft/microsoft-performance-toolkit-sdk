// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for comparing
    ///     objects.
    /// </summary>
    public static class ComparableUtils
    {
        /// <summary>
        ///     This is a helper method for implementing IComparable.CompareTo() when you've already implemented IComparable&lt;TThis&gt;.
        /// </summary>
        /// <param name="this">The left side of the comparison.</param>
        /// <param name="obj">The right side of the comparison.</param>
        /// <returns>
        ///     An integer value less than zero (0) if <paramref name="this"/> is
        ///     considered to be less than <paramref name="obj"/>;
        ///     An integer value equal to zero (0) if <paramref name="this"/> is
        ///     considered to be equal to <paramref name="obj"/>;
        ///     An integer value greater than zero (0) if <paramref name="this"/> is
        ///     considered to be greater than <paramref name="obj"/>.
        /// </returns>
        public static int CompareTo<TThis>(TThis @this, object obj)
            where TThis : IComparable, IComparable<TThis>
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is TThis)
            {
                return @this.CompareTo((TThis)obj);
            }
            else
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    "obj is not the correct type (expected {0}, actual {1})",
                    typeof(TThis).FullName,
                    obj.GetType().FullName);

                throw new ArgumentException(message, nameof(obj));
            }
        }
    }
}
