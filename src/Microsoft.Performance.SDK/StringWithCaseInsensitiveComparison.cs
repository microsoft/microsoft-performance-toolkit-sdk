// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a string that compares without regards
    ///     to case sensitivity.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance.SDK", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public readonly struct StringWithCaseInsensitiveComparison
        : IComparable<StringWithCaseInsensitiveComparison>,
          IComparable,
          IFormattable
    {
        private readonly string? source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringWithCaseInsensitiveComparison"/>
        ///     class.
        /// </summary>
        /// <param name="source">
        ///     The underlying string value.
        /// </param>
        public StringWithCaseInsensitiveComparison(string source)
        {
            this.source = source;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.Boolean,System.Globalization.CultureInfo)")]
        public int CompareTo(StringWithCaseInsensitiveComparison other)
        {
            return string.Compare(this.source, other.source, true, CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        int IComparable.CompareTo(object other)
        {
            return ComparableUtils.CompareTo(this, other);
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="StringWithCaseInsensitiveComparison"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="StringWithCaseInsensitiveComparison"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="StringWithCaseInsensitiveComparison"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(StringWithCaseInsensitiveComparison first, StringWithCaseInsensitiveComparison second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="StringWithCaseInsensitiveComparison"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="StringWithCaseInsensitiveComparison"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="StringWithCaseInsensitiveComparison"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(StringWithCaseInsensitiveComparison first, StringWithCaseInsensitiveComparison second)
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
