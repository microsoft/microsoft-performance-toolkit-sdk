// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a signed quantity of bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(SignedBytesConverter))]
    public struct SignedBytes
        : IComparable<SignedBytes>,
          IEquatable<SignedBytes>,
          IComparable,
          IPlottableGraphType,
          IConvertible
    {
        private long bytes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SignedBytes"/>
        ///     struct.
        /// </summary>
        /// <param name="bytes">
        ///     The count of bytes.
        /// </param>
        public SignedBytes(long bytes)
        {
            this.bytes = bytes;
        }

        /// <summary>
        ///     Gets the total number of bytes represented
        ///     by this instance.
        /// </summary>
        public long TotalBytes
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
                return SignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Kilobytes);
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
                return SignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Megabytes);
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
                return SignedBytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Gigabytes);
            }
        }

        /// <summary>
        ///     Gets the value representing zero (0) bytes.
        /// </summary>
        public static SignedBytes Zero
        {
            get
            {
                return default(SignedBytes);
            }
        }

        /// <summary>
        ///     Gets the minimum number of bytes representable by this
        ///     struct.
        /// </summary>
        public static SignedBytes MinValue
        {
            get
            {
                return new SignedBytes(long.MinValue);
            }
        }

        /// <summary>
        ///     Gets the maximum number of bytes representable by this
        ///     struct.
        /// </summary>
        public static SignedBytes MaxValue
        {
            get
            {
                return new SignedBytes(long.MaxValue);
            }
        }

        /// <inheritdoc />
        public int CompareTo(SignedBytes other)
        {
            return this.bytes.CompareTo(other.bytes);
        }

        /// <inheritdoc />
        public bool Equals(SignedBytes other)
        {
            return this.bytes.Equals(other.bytes);
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="SignedBytes"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(SignedBytes first, SignedBytes second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="SignedBytes"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(SignedBytes first, SignedBytes second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="SignedBytes"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(SignedBytes first, SignedBytes second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="SignedBytes"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(SignedBytes first, SignedBytes second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="SignedBytes"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(SignedBytes first, SignedBytes second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="SignedBytes"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="SignedBytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(SignedBytes first, SignedBytes second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is SignedBytes) && Equals((SignedBytes)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.bytes.GetHashCode();
        }

        /// <summary>
        ///     Subtracts two quantities of <see cref="SignedBytes"/> from
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
        public static SignedBytes operator -(SignedBytes first, SignedBytes second)
        {
            return new SignedBytes(unchecked(first.bytes - second.bytes));
        }

        /// <summary>
        ///     Adds two quantities of <see cref="SignedBytes"/> to
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
        public static SignedBytes operator +(SignedBytes first, SignedBytes second)
        {
            return new SignedBytes(unchecked(first.bytes + second.bytes));
        }

        /// <inheritdoc />
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(LargeSignedBytes))
            {
                return new LargeSignedBytes(this.TotalBytes);
            }
            else if (conversionType == typeof(SignedBytes)) // needed since Convert.ToType goes down this code-path
            {
                return this;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        /// <inheritdoc />
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <inheritdoc />
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this.bytes);
        }

        /// <inheritdoc />
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this.bytes);
        }

        /// <inheritdoc />
        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this.bytes);
        }

        /// <inheritdoc />
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(this.bytes);
        }

        /// <inheritdoc />
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.bytes;
        }

        /// <inheritdoc />
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this.bytes);
        }

        /// <inheritdoc />
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this.bytes);
        }

        /// <inheritdoc />
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this.bytes);
        }

        /// <inheritdoc />
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this.bytes);
        }

        /// <inheritdoc />
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this.bytes);
        }

        /// <inheritdoc />
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this.bytes);
        }

        /// <inheritdoc />
        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.ToString();
        }

        /// <inheritdoc />
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this.bytes);
        }

        /// <inheritdoc />
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this.bytes);
        }

        /// <inheritdoc />
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this.bytes);
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
