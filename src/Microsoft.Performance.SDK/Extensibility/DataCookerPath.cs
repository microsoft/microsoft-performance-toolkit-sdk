// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     This helper struct provides methods to manipulate a string in the form of a path to
    ///     an <see cref="T:Microsoft.Performance.SDK.Extensibility.DataCooking.IDataCooker" />.
    /// </summary>
    public struct DataCookerPath
        : IEquatable<DataCookerPath>
    {
        /// <summary>
        ///     If the data cooker is not a source data cooker, this is the source parser Id.
        /// </summary>
        /// <remarks>
        ///    This must match 
        ///    Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers.DataCookerPathInternal.EmptySourceParserId.
        /// </remarks>
        private static readonly string EmptySourceParserId = string.Empty;

        /// <summary>
        ///     Separates the consituent parts of the data cooker path in the full string representation. This character is not
        ///     allowed in path components. If this path is changed, please update the exception comments in <see cref="ForSource(string, string)"/>
        ///     and <see cref="ForComposite(string)"/>.
        /// </summary>
        private static readonly string Separator = "/";

        private DataCookerPath(string dataCookerId)
            : this(EmptySourceParserId, dataCookerId)
        {
        }

        private DataCookerPath(string sourceParserId, string dataCookerId)
        {
            Debug.Assert((sourceParserId == EmptySourceParserId) || !string.IsNullOrWhiteSpace(sourceParserId));
            Debug.Assert(!string.IsNullOrWhiteSpace(dataCookerId));

            if (sourceParserId.Contains(Separator))
            {
                throw new ArgumentException($"This value may not contain a '{Separator}'.", nameof(sourceParserId));
            }

            if (dataCookerId.Contains(Separator))
            {
                throw new ArgumentException($"This value may not contain a '{Separator}'.", nameof(dataCookerId));
            }

            this.SourceParserId = string.Intern(sourceParserId);
            this.DataCookerId = string.Intern(dataCookerId);
            this.DataCookerType = SourceParserId == EmptySourceParserId
                ? DataCookerType.CompositeDataCooker
                : DataCookerType.SourceDataCooker;

            this.CookerPath = this.SourceParserId + Separator + this.DataCookerId;
        }

        /// <summary>
        ///     Constructor that takes an existing DataCookerPath and duplicates the values.
        /// </summary>
        /// <param name="other">
        ///     An existing data cooker path.
        /// </param>
        public DataCookerPath(DataCookerPath other)
        {
            Guard.NotNull(other, nameof(other));

            this.SourceParserId = other.SourceParserId;
            this.DataCookerId = other.DataCookerId;
            this.DataCookerType = other.DataCookerType;
            this.CookerPath = other.CookerPath;
        }

        /// <summary>
        ///     Gets the path of this cooker.
        /// </summary>
        internal string CookerPath { get; }

        /// <summary>
        ///     Creates a new <see cref="DataCookerType.CompositeDataCooker"/> path.
        /// </summary>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     One or more of the parameters is empty, composed only of whitespace, 
        ///     or contains the '/' character.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataCookerId"/> is null.
        /// </exception>
        public static DataCookerPath ForComposite(string dataCookerId)
        {
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            return new DataCookerPath(dataCookerId);
        }

        /// <summary>
        ///     Creates a new <see cref="DataCookerType.SourceDataCooker"/> path.
        /// </summary>
        /// <param name="sourceParserId">
        ///     The ID of the source parser.
        /// </param>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     One or more of the parameters is empty, composed only of whitespace, 
        ///     or contains the '/' character.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        public static DataCookerPath ForSource(string sourceParserId, string dataCookerId)
        {
            if (sourceParserId != EmptySourceParserId)
            {
                Guard.NotNullOrWhiteSpace(sourceParserId, nameof(sourceParserId));
            }

            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            return new DataCookerPath(sourceParserId, dataCookerId);
        }

        /// <summary>
        ///     Gets the type of the data cooker that this path targets.
        ///     <seealso cref="DataCookerType"/> for more information about what
        ///     different types of data cookers mean.
        /// </summary>
        public DataCookerType DataCookerType
        {
            get;
        }

        /// <summary>
        ///     Gets the source parser Id.
        /// </summary>
        public string SourceParserId { get; }

        /// <summary>
        ///     Gets the Data cooker Id.
        /// </summary>
        public string DataCookerId { get; }

        /// <summary>
        ///     Compares two instance of <see cref="DataCookerPath"/> for equality.
        /// </summary>
        /// <param name="left">
        ///     The first instance to compare.
        /// </param>
        /// <param name="right">
        ///     The second instance to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if both instances are considered to be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(DataCookerPath left, DataCookerPath right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Compares two instance of <see cref="DataCookerPath"/> for inequality.
        /// </summary>
        /// <param name="left">
        ///     The first instance to compare.
        /// </param>
        /// <param name="right">
        ///     The second instance to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if both instances are considered to not be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(DataCookerPath left, DataCookerPath right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.CookerPath;
        }

        /// <inheritdoc />
        public bool Equals(DataCookerPath other)
        {
            return string.Equals(this.CookerPath, other.CookerPath);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.CookerPath?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DataCookerPath other && Equals(other);
        }
    }
}