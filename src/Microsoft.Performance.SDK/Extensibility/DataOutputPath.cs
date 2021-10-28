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
        /// <exception cref="ArgumentException">
        ///     One or more of the parameters is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        public static DataOutputPath ForSource(
            string sourceParserId, 
            string dataCookerId, 
            string dataOutputId)
        {
            Guard.NotNullOrWhiteSpace(sourceParserId, nameof(sourceParserId));
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));
            Guard.NotNullOrWhiteSpace(dataOutputId, nameof(dataOutputId));

            var dataCookerPath = DataCookerPath.ForSource(sourceParserId, dataCookerId);

            return new DataOutputPath(dataCookerPath, dataOutputId);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputPath"/>
        ///     struct with the given data cooker ID and output ID.
        /// </summary>
        /// <param name="dataCookerId">
        ///     Data cooker Id.
        /// </param>
        /// <param name="dataOutputId">
        ///     Data output Id.
        /// </param>
        /// <returns>
        ///     DataOutputPath object combined from the given parameters.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     One or more of the parameters is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     One or more of the parameters is null.
        /// </exception>
        public static DataOutputPath ForComposite(
            string dataCookerId,
            string dataOutputId)
        {
            Guard.NotNullOrWhiteSpace(dataCookerId, nameof(dataCookerId));
            Guard.NotNullOrWhiteSpace(dataOutputId, nameof(dataOutputId));

            var dataCookerPath = DataCookerPath.ForComposite(dataCookerId);

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
        /// <exception cref="ArgumentException">
        ///     <paramref name="outputId"/> is empty or composed only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="outputId"/> is null.
        /// </exception>
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
        /// <remarks>
        ///     This may be null or <see cref="string.Empty"/> if this <see cref="DataOutputPath"/>
        ///     is for a composite cooker.
        /// </remarks>
        public string SourceParserId => CookerPath.SourceParserId;

        /// <summary>
        ///     Identifies the data cooker associated with this data.
        /// </summary>
        public string DataCookerId => CookerPath.DataCookerId;

        /// <inheritdoc />
        public override string ToString()
        {
            return this.CookerPath.CookerPath + "/" + this.DataCookerId;
        }
    }
}
