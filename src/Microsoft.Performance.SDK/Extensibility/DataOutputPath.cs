// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     This helper class provides methods to manipulate string in the form of paths to
    ///     data output from a data cooker.
    /// </summary>
    public struct DataOutputPath
    {
        /// <summary>
        ///     Describes the format of a data output path.
        /// </summary>
        public static string Format => "[DataCookerPath]/OutputId";

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputPath"/>
        ///     with the given path.
        /// </summary>
        /// <param name="path">
        ///     Well formatted data output path.
        /// </param>
        /// <returns>
        ///     DataOutputPath object which represents the provided string path.
        /// </returns>
        public static DataOutputPath Create(
            string path)
        {
            Guard.NotNullOrWhiteSpace(path, nameof(path));

            var success = TryGetConstituents(path, out var sourceId, out var cookerId, out var outputId);
            if (!success)
            {
                throw new ArgumentException("Invalid data output path", nameof(path));
            }

            return Create(sourceId, cookerId, outputId);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputPath"/>
        ///     struct with the given source parser ID, data cooker ID, and
        ///     output ID.
        /// </summary>
        /// <param name="sourceParserId">
        ///     Source parser Id.
        /// </param>
        /// <param name="dataCookerId">
        ///     Data cooker Id.
        /// </param>
        /// <param name="dataOutputId">
        ///     Data output Id.
        /// </param>
        /// <returns>
        ///     DataOutputPath object combined from the given parameters.
        /// </returns>
        public static DataOutputPath Create(
            string sourceParserId, 
            string dataCookerId, 
            string dataOutputId)
        {
            Guard.NotNullOrWhiteSpace(dataOutputId, nameof(dataOutputId));

            var dataCookerPath = new DataCookerPath(sourceParserId, dataCookerId);

            return new DataOutputPath(dataCookerPath, dataOutputId);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputPath"/>
        ///     struct with the given cooker path and output ID.
        /// </summary>
        /// <param name="cookerPath">
        ///     Data cooker path.
        /// </param>
        /// <param name="outputId">
        ///     Data output Id.
        /// </param>
        public DataOutputPath(DataCookerPath cookerPath, string outputId)
        {
            Guard.NotNullOrWhiteSpace(outputId, nameof(outputId));

            if (outputId.Contains("/"))
            {
                throw new ArgumentException("This value may not contain a '/'.", nameof(outputId));
            }

            this.CookerPath = cookerPath;
            this.OutputId = string.Intern(outputId);
        }

        /// <summary>
        ///     Gets the data output identifier.
        /// </summary>
        public string OutputId { get; }

        /// <summary>
        ///     Gets the path to data cooker in which the output data is stored.
        /// </summary>
        public DataCookerPath CookerPath { get; }

        /// <summary>
        ///     Gets the identifier of the source parser associated with this data.
        /// </summary>
        public string SourceParserId => CookerPath.SourceParserId;

        /// <summary>
        ///     Identifies the data cooker associated with this data.
        /// </summary>
        public string DataCookerId => CookerPath.DataCookerId;

        /// <summary>
        ///     The complete path to the data output.
        /// </summary>
        public string Path => this.CookerPath.CookerPath + "/" + this.OutputId;

        /// <summary>
        ///     Generates a data cooker path given a source parser Id and a data cooker Id.
        /// </summary>
        /// <param name="sourceParserId">
        ///     Source parser Id.
        /// </param>
        /// <param name="dataCookerId">
        ///     Data cooker Id.
        /// </param>
        /// <param name="dataOutputId">
        ///     Data output identifier.
        /// </param>
        /// <returns>
        ///     The path.
        /// </returns>
        public static string Combine(string sourceParserId, string dataCookerId, string dataOutputId)
        {
            Guard.NotNullOrWhiteSpace(dataOutputId, nameof(dataOutputId));

            return DataCookerPath.Create(sourceParserId, dataCookerId) + "/" + dataOutputId;
        }

        /// <summary>
        ///     Returns the source parser Id from a data output path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <returns>
        ///     Source parser Id.
        /// </returns>
        public static string GetSourceId(string dataOutputPath)
        {
            Guard.NotNullOrWhiteSpace(dataOutputPath, nameof(dataOutputPath));

            var tokens = SplitPath(dataOutputPath);
            if (tokens != null)
            {
                return tokens[0];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Returns the data cooker Id from a data output path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <returns>
        ///     Data cooker Id.
        /// </returns>
        public static string GetDataCookerId(string dataOutputPath)
        {
            Guard.NotNullOrWhiteSpace(dataOutputPath, nameof(dataOutputPath));

            var tokens = SplitPath(dataOutputPath);
            if (tokens != null)
            {
                return tokens[1];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Returns the data output Id from a data output path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <returns>
        ///     Data output Id.
        /// </returns>
        public static string GetDataOutputId(string dataOutputPath)
        {
            Guard.NotNullOrWhiteSpace(dataOutputPath, nameof(dataOutputPath));

            var tokens = SplitPath(dataOutputPath);
            if (tokens != null)
            {
                return tokens[2];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Returns the data cooker path from a data output path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <returns>
        ///     Data cooker path.
        /// </returns>
        public static DataCookerPath GetDataCookerPath(string dataOutputPath)
        {
            Guard.NotNullOrWhiteSpace(dataOutputPath, nameof(dataOutputPath));

            var tokens = SplitPath(dataOutputPath);
            if (tokens != null)
            {
                return new DataCookerPath(tokens[0], tokens[1]);
            }

            throw new ArgumentException("Not a valid data output path.", nameof(dataOutputPath));
        }

        /// <summary>
        ///     Splits a data output path into individual identifiers.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <param name="sourceParserId">
        ///     Source parser identifier.
        /// </param>
        /// <param name="dataCookerId">
        ///     Data cooker identifier.
        /// </param>
        /// <param name="dataOutputId">
        ///     Data output identifier.
        /// </param>
        /// <returns>
        ///     true if the path was parsed; false otherwise.
        /// </returns>
        public static bool TryGetConstituents(
            string dataOutputPath, 
            out string sourceParserId, 
            out string dataCookerId, 
            out string dataOutputId)
        {
            sourceParserId = string.Empty;
            dataCookerId = string.Empty;
            dataOutputId = string.Empty;

            var tokens = SplitPath(dataOutputPath);
            if (tokens != null)
            {
                sourceParserId = tokens[0];
                dataCookerId = tokens[1];
                dataOutputId = tokens[2];

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determines if a data output path has a proper form.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     Data output path.
        /// </param>
        /// <returns>
        ///     True for a well formed path, false otherwise.
        /// </returns>
        public static bool IsWellFormed(string dataOutputPath)
        {
            if (string.IsNullOrWhiteSpace(dataOutputPath))
            {
                return false;
            }

            return SplitPath(dataOutputPath) != null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Path;
        }

        private static string[] SplitPath(string outputPath)
        {
            var tokens = outputPath.Split('/');
            if (tokens.Length == 3)
            {
                return tokens;
            }

            return null;
        }
    }
}
