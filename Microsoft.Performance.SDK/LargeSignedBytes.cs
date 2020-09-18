// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a large signed quantity of bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(LargeSignedBytesConverter))]
    public readonly struct LargeSignedBytes
        : IComparable<LargeSignedBytes>,
          IEquatable<LargeSignedBytes>,
          IComparable,
          IPlottableGraphType
    {
        private readonly decimal bytes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LargeSignedBytes"/>
        ///     struct.
        /// </summary>
        /// <param name="bytes">
        ///     The count of bytes.
        /// </param>
        public LargeSignedBytes(decimal bytes)
        {
            this.bytes = bytes;
        }

        /// <summary>
        ///     Gets the total number of bytes represented
        ///     by this instance.
        /// </summary>
        public decimal TotalBytes
        {
            get
            {
                return this.bytes;
            }
        }

        /// <summary>
        ///     Gets the total number of kilobytes represented
        ///     by this instance.
        /// </summary>
        public decimal TotalKilobytes
        {
            get
            {
                return LargeSignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Kilobytes);
            }
        }

        /// <summary>
        ///     Gets the total number of megabytes represented
        ///     by this instance.
        /// </summary>
        public decimal TotalMegabytes
        {
            get
            {
                return LargeSignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Megabytes);
            }
        }

        /// <summary>
        ///     Gets the total number of gigabytes represented
        ///     by this instance.
        /// </summary>
        public decimal TotalGigabytes
        {
            get
            {
                return LargeSignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Gigabytes);
            }
        }

        /// <summary>
        ///     Gets the value representing zero (0) bytes.
        /// </summary>
        public static LargeSignedBytes Zero
        {
            get
            {
                return default(LargeSignedBytes);
            }
        }

        /// <summary>
        ///     Gets the minimum number of bytes representable by this
        ///     struct.
        /// </summary>
        public static LargeSignedBytes MinValue
        {
            get
            {
                return new LargeSignedBytes(decimal.MinValue);
            }
        }

        /// <summary>
        ///     Gets the maximum number of bytes representable by this
        ///     struct.
        /// </summary>
        public static LargeSignedBytes MaxValue
        {
            get
            {
                return new LargeSignedBytes(decimal.MaxValue);
            }
        }

        /// <inheritdoc />
        public int CompareTo(LargeSignedBytes other)
        {
            return this.bytes.CompareTo(other.bytes);
        }

        /// <inheritdoc />
        public bool Equals(LargeSignedBytes other)
        {
            return this.bytes.Equals(other.bytes);
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="LargeSignedBytes"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(LargeSignedBytes first, LargeSignedBytes second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="LargeSignedBytes"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(LargeSignedBytes first, LargeSignedBytes second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="LargeSignedBytes"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(LargeSignedBytes first, LargeSignedBytes second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="LargeSignedBytes"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(LargeSignedBytes first, LargeSignedBytes second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="LargeSignedBytes"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(LargeSignedBytes first, LargeSignedBytes second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="LargeSignedBytes"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="LargeSignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(LargeSignedBytes first, LargeSignedBytes second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is LargeSignedBytes) && Equals((LargeSignedBytes)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.bytes.GetHashCode();
        }

        /// <summary>
        ///     Subtracts two quantities of <see cref="LargeSignedBytes"/> from
        ///     each other.
        /// </summary>
        /// <param name="first">
        ///     The minuend.
        /// </param>
        /// <param name="second">
        ///     The subtrahend.
        /// </param>
        /// <returns>
        ///     The result of the subtraction of <paramref name="second"/> from
        ///     <paramref name="first"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static LargeSignedBytes operator -(LargeSignedBytes first, LargeSignedBytes second)
        {
            return new LargeSignedBytes(unchecked(first.bytes - second.bytes));
        }

        /// <summary>
        ///     Adds two quantities of <see cref="LargeSignedBytes"/> to
        ///     each other.
        /// </summary>
        /// <param name="first">
        ///     The first addend.
        /// </param>
        /// <param name="second">
        ///     The second addend.
        /// </param>
        /// <returns>
        ///     The result of the addition of <paramref name="first"/> with
        ///     <paramref name="second"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static LargeSignedBytes operator +(LargeSignedBytes first, LargeSignedBytes second)
        {
            return new LargeSignedBytes(unchecked(first.bytes + second.bytes));
        }

        /// <inheritdoc />
        int IComparable.CompareTo(object obj)
        {
            return ComparableUtils.CompareTo(this, obj);
        }

        /// <inheritdoc />
        public double GetGraphValue()
        {
            return (double)this.bytes;
        }
    }
}
