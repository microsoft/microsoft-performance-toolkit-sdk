// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

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
        private const string EmptySourceParserId = "";

        /// <summary>
        ///     This is used by data cooker paths to compare elements.
        /// </summary>
        public static IEqualityComparer<string> EqualityComparer = StringComparer.Ordinal;

        /// <summary>
        ///     Constructor without an empty source parser id.
        ///     Used for composite data cookers.
        /// </summary>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataCookerId"/> is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataCookerId"/> is null.
        /// </exception>
        private DataCookerPath(string dataCookerId)
        {
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            if (dataCookerId.Contains("/"))
            {
                throw new ArgumentException("This value may not contain a '/'.", nameof(dataCookerId));
            }

            this.SourceParserId = DataCookerPath.EmptySourceParserId;
            this.DataCookerId = string.Intern(dataCookerId);
            this.CookerPath = Create(DataCookerPath.EmptySourceParserId, dataCookerId);
            this.DataCookerType = DataCookerType.CompositeDataCooker;
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
        ///     One or more of the parameters is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        private DataCookerPath(string sourceParserId, string dataCookerId)
        {
            Guard.NotNullOrWhiteSpace(sourceParserId, nameof(sourceParserId));
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            if (sourceParserId.Contains("/"))
            {
                throw new ArgumentException("This value may not contain a '/'.", nameof(sourceParserId));
            }

            if (dataCookerId.Contains("/"))
            {
                throw new ArgumentException("This value may not contain a '/'.", nameof(dataCookerId));
            }

            this.SourceParserId = string.Intern(sourceParserId);
            this.DataCookerId = string.Intern(dataCookerId);
            this.CookerPath = Create(sourceParserId, dataCookerId);
            this.DataCookerType = DataCookerType.SourceDataCooker;
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
            this.CookerPath = other.CookerPath;
            this.DataCookerType = other.DataCookerType;
        }

        /// <summary>
        ///     Creates a new <see cref="DataCookerType.CompositeDataCooker"/> path.
        /// </summary>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataCookerId"/> is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataCookerId"/> is null.
        /// </exception>
        public static DataCookerPath ForComposite(string dataCookerId)
        {
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
        ///     One or more of the parameters is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        public static DataCookerPath ForSource(string sourceParserId, string dataCookerId)
        {
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
        ///     Gets the combination of the source parser Id and the data cooker Id.
        /// </summary>
        internal string CookerPath
        {
            get;
        }

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
            return DataCookerPath.EqualityComparer.Equals(this.CookerPath, other.CookerPath);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.CookerPath != null ? this.CookerPath.GetHashCode() : 0);
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

        /// <summary>
        ///     Generates a data cooker path given a source parser Id and a data cooker Id.
        /// </summary>
        /// <param name="sourceParserId">
        ///     Source parser Id.
        /// </param>
        /// <param name="dataCookerId">
        ///     Data cooker Id.
        /// </param>
        /// <returns>
        ///     The created data cooker path.
        /// </returns>
        internal static string Create(string sourceParserId, string dataCookerId)
        {
            Guard.NotNull(sourceParserId, nameof(sourceParserId));
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            return sourceParserId + "/" + dataCookerId;
        }
    }
}
