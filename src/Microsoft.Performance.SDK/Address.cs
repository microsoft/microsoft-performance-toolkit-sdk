// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a memory address.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(AddressConverter))]
    [DebuggerDisplay("{" + nameof(address) + "}")]
    public readonly struct Address
        : IComparable<Address>,
          IEquatable<Address>,
          IComparable
    {
        private readonly ulong address;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Address"/>
        ///     class with the given memory address.
        /// </summary>
        /// <param name="address">
        ///     The memory address.
        /// </param>
        public Address(ulong address)
        {
            this.address = address;
        }

        /// <summary>
        /// Returns the raw address value.
        /// </summary>
        public ulong Value => this.address;

        /// <summary>
        ///     Gets the byte representation of this instance.
        /// </summary>
        public ulong ToBytes => this.address;

        /// <summary>
        ///     Gets the <see cref="Address"/> that represents
        ///     memory address zero (0.)
        /// </summary>
        public static Address Zero => default(Address);

        /// <summary>
        ///     Gets the value of the lowest memory address
        ///     representable by this class.
        /// </summary>
        public static Address MinValue => new Address(ulong.MinValue);

        /// <summary>
        ///     Gets the value of the highest memory address
        ///     representable by this class.
        /// </summary>
        public static Address MaxValue => new Address(ulong.MaxValue);

        /// <inheritdoc />
        public int CompareTo(Address other)
        {
            return this.address.CompareTo(other.address);
        }

        /// <inheritdoc />
        public bool Equals(Address other)
        {
            return this.address == other.address;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Address"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(Address first, Address second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Address"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(Address first, Address second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Address"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(Address first, Address second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Address"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(Address first, Address second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="Address"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(Address first, Address second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="Address"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Address"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Address"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(Address first, Address second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        ///     Add bytes to a given address to create a new address.
        /// </summary>
        /// <param name="left">
        ///     Existing address.
        /// </param>
        /// <param name="bytes">
        ///     Byte count to add.
        /// </param>
        /// <returns>
        ///     Adjusted address.
        /// </returns>
        public static Address operator +(Address left, Bytes bytes)
        {
            checked
            {
                return new Address(left.Value + bytes.TotalBytes);
            }
        }

        /// <summary>
        ///     Subtract bytes from a given address to create a new address.
        /// </summary>
        /// <param name="left">
        ///     Existing address.
        /// </param>
        /// <param name="bytes">
        ///     Byte count to add.
        /// </param>
        /// <returns>
        ///     Adjusted address.
        /// </returns>
        public static Address operator -(Address left, Bytes bytes)
        {
            checked
            {
                return new Address(left.Value - bytes.TotalBytes);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is Address) && Equals((Address)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.address.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.address.ToString();
        }

        /// <inheritdoc />
        int IComparable.CompareTo(object obj)
        {
            return ComparableUtils.CompareTo(this, obj);
        }
    }
}
