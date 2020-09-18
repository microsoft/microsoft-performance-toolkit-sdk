// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a quantity as a number of bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(BytesConverter))]
    public readonly struct Bytes
        : IComparable<Bytes>,
          IEquatable<Bytes>,
          IComparable,
          IPlottableGraphType,
          IConvertible,
          IDiffConvertible<LargeSignedBytes>
    {
        private readonly ulong bytes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Bytes"/>
        ///     class, representing the given number of bytes.
        /// </summary>
        /// <param name="bytes">
        ///     The number of bytes being represented.
        /// </param>
        public Bytes(ulong bytes)
        {
            this.bytes = bytes;
        }

        /// <summary>
        ///     Gets the total number of bytes represented by this instance.
        /// </summary>
        public ulong TotalBytes
        {
            get
            {
                return this.bytes;
            }
        }

        /// <summary>
        ///     Gets the total number of bytes represented by this instance,
        ///     expressed as a value in kilobytes.
        /// </summary>
        public decimal TotalKilobytes
        {
            get
            {
                return BytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Kilobytes);
            }
        }

        /// <summary>
        ///     Gets the total number of bytes represented by this instance,
        ///     expressed as a value in megabytes.
        /// </summary>
        public decimal TotalMegabytes
        {
            get
            {
                return BytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Megabytes);
            }
        }

        /// <summary>
        ///     Gets the total number of bytes represented by this instance,
        ///     expressed as a value in gigabytes.
        /// </summary>
        public decimal TotalGigabytes
        {
            get
            {
                return BytesConverter.ConvertToBytesUnit(this.bytes, BytesUnits.Gigabytes);
            }
        }

        /// <summary>
        ///     Gets the instance representing a quantity of zero (0)
        ///     bytes.
        /// </summary>
        public static Bytes Zero
        {
            get
            {
                return default(Bytes);
            }
        }

        /// <summary>
        ///     Gets the smallest quantity of bytes representable by
        ///     this class.
        /// </summary>
        public static Bytes MinValue
        {
            get
            {
                return new Bytes(ulong.MinValue);
            }
        }

        /// <summary>
        ///     Gets the largest quantity of bytes representable by
        ///     this class.
        /// </summary>
        public static Bytes MaxValue
        {
            get
            {
                return new Bytes(ulong.MaxValue);
            }
        }

        /// <inheritdoc />
        public int CompareTo(Bytes other)
        {
            return this.bytes.CompareTo(other.bytes);
        }

        /// <inheritdoc />
        public bool Equals(Bytes other)
        {
            return this.bytes.Equals(other.bytes);
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Bytes"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(Bytes first, Bytes second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Bytes"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(Bytes first, Bytes second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Bytes"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(Bytes first, Bytes second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Bytes"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(Bytes first, Bytes second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="Bytes"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(Bytes first, Bytes second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="Bytes"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Bytes"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Bytes"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(Bytes first, Bytes second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is Bytes) && Equals((Bytes)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.bytes.GetHashCode();
        }

        /// <summary>
        ///     Subtracts two quantities of <see cref="Bytes"/> from
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
        public static Bytes operator -(Bytes first, Bytes second)
        {
            return new Bytes(unchecked(first.bytes - second.bytes));
        }

        /// <summary>
        ///     Adds two quantities of <see cref="Bytes"/> to
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
        public static Bytes operator +(Bytes first, Bytes second)
        {
            return new Bytes(unchecked(first.bytes + second.bytes));
        }

        /// <inheritdoc />
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(LargeSignedBytes))
            {
                return new LargeSignedBytes(this.TotalBytes);
            }
            else if (conversionType == typeof(Bytes)) // needed since Convert.ToType goes down this code-path
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

        /// <summary>
        ///     Gets a graphable representation of this instance's
        ///     value.
        /// </summary>
        /// <returns>
        ///     The value of this instance as a <see cref="double"/>,
        ///     suitable for graphing.
        /// </returns>
        public double GetGraphValue()
        {
            return (double)this.bytes;
        }
    }
}
