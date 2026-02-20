// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a string that is compared logically.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance.SDK", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public readonly struct StringWithLogicalComparison
        : IComparable<StringWithLogicalComparison>,
          IComparable,
          IFormattable
    {
        private readonly string? source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringWithLogicalComparison"/>
        ///     class.
        /// </summary>
        /// <param name="source">
        ///     The underlying string value.
        /// </param>
        public StringWithLogicalComparison(string source)
        {
            this.source = source;
        }

        /// <inheritdoc />
        public int CompareTo(StringWithLogicalComparison other)
        {
            return CompareWithNumericExceptionMethods._wcscmpWithNumericExceptions(this.source, other.source);
        }

        /// <inheritdoc />
        int IComparable.CompareTo(object other)
        {
            return ComparableUtils.CompareTo(this, other);
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="StringWithLogicalComparison"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="StringWithLogicalComparison"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="StringWithLogicalComparison"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(StringWithLogicalComparison first, StringWithLogicalComparison second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="StringWithLogicalComparison"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="StringWithLogicalComparison"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="StringWithLogicalComparison"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(StringWithLogicalComparison first, StringWithLogicalComparison second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.source ?? string.Empty;
        }

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }
    }
}
