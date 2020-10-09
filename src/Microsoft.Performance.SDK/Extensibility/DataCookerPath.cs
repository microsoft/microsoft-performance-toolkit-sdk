// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

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
        ///     Describes the format of a data cooker path.
        /// </summary>
        public static string Format => "SourceId/CookerId";

        /// <summary>
        ///     If the data cooker is not a source data cooker, this is the source parser Id.
        /// </summary>
        public const string EmptySourceParserId = "";

        /// <summary>
        ///     Constructor without an empty source parser id.
        ///     Used for composite data cookers.
        /// </summary>
        /// <param name="dataCookerId">
        ///     The ID of the data cooker.
        /// </param>
        public DataCookerPath(string dataCookerId)
            : this(EmptySourceParserId, dataCookerId)
        {
        }

        /// <summary>
        ///     Constructor that takes a source parser id. If that ID is not empty, this
        ///     is used for source data cookers. If it is empty, this is used with composite data cookers.
        /// </summary>
        /// <param name="sourceParserId">
        ///     Source parser Id.
        /// </param>
        /// <param name="dataCookerId">
        ///     Data cooker Id.
        /// </param>
        public DataCookerPath(string sourceParserId, string dataCookerId)
        {
            Guard.NotNull(sourceParserId, nameof(sourceParserId));
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
        /// Gets the combination of the source parser Id and the data cooker Id.
        /// </summary>
        public string CookerPath { get; }

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
        ///     Generates a data cooker path given a full cooker path.
        /// </summary>
        /// <param name="cookerPath">
        ///     The full cooker path. See <see cref="Format"/> for details.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="DataCookerPath"/>.
        /// </returns>
        public static DataCookerPath Parse(string cookerPath)
        {
            Guard.NotNullOrWhiteSpace(cookerPath, nameof(cookerPath));

            var split = cookerPath.Split('/');
            if (split.Length == 1)
            {
                return new DataCookerPath(split[0]);
            }

            if (split.Length == 2)
            {
                return new DataCookerPath(split[0], split[1]);
            }

            throw new FormatException();
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
        public static string Create(string sourceParserId, string dataCookerId)
        {
            Guard.NotNull(sourceParserId, nameof(sourceParserId));
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));

            return sourceParserId + "/" + dataCookerId;
        }

        /// <summary>
        ///     Returns the source parser Id from a data cooker path.
        /// </summary>
        /// <param name="cookerPath">
        ///     Data cooker path.
        /// </param>
        /// <returns>
        ///     Source parser Id.
        /// </returns>
        public static string GetSourceParserId(string cookerPath)
        {
            Guard.NotNullOrWhiteSpace(cookerPath, nameof(cookerPath));

            var tokens = SplitPath(cookerPath);
            if (tokens != null)
            {
                return tokens[0];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Returns the data cooker Id from a data cooker path.
        /// </summary>
        /// <param name="cookerPath">
        ///     Data cooker path.
        /// </param>
        /// <returns>
        ///     Data cooker Id.
        /// </returns>
        public static string GetDataCookerId(string cookerPath)
        {
            Guard.NotNullOrWhiteSpace(cookerPath, nameof(cookerPath));

            var tokens = SplitPath(cookerPath);
            if (tokens != null)
            {
                return tokens[1];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Determines if a data cooker path has a proper form.
        /// </summary>
        /// <param name="cookerPath">
        ///     Data cooker path.
        /// </param>
        /// <returns>
        ///     True for a well formed path, false otherwise.
        /// </returns>
        public static bool IsWellFormed(string cookerPath)
        {
            if (string.IsNullOrWhiteSpace(cookerPath))
            {
                return false;
            }

            return SplitPath(cookerPath) != null;
        }

        private static string[] SplitPath(string cookerPath)
        {
            var tokens = cookerPath.Split('/');
            if (tokens.Length == 2)
            {
                return tokens;
            }

            return null;
        }
    }
}
